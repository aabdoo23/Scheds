using Microsoft.AspNetCore.Identity.Data;
using Scheds.Models;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

namespace Scheds.DAL.Services
{
    public static class GenerationHelper
    {
        private static bool HasSubsections(List<CardItem> course)
        {
            foreach (var card in course)
            {
                if (!card.isMainSection())
                {
                    return true;
                }
            }
            return false;
        }
        private static bool HasLabAndTutorial(List<CardItem> course)
        {
            Dictionary<string, int> freq = new Dictionary<string, int>();

            foreach (var card in course)
            {
                if (card.isMainSection()) continue;
                string section = card.Section.Substring(0, 3);

                if (freq.TryGetValue(section, out int count))
                {
                    freq[section] = count + 1;
                }
                else
                {
                    freq[section] = 1;
                }

                if (freq[section] > 1)
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetTimeDiffInMins(TimeSpan t1, TimeSpan t2)
        {
            return (int)(t2 - t1).TotalMinutes;
        }

        private static bool PassesTimeGapConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (request.largestAllowedGap == 0) return true;

            foreach (var day in ItemsPerDay)
            {
                List<CardItem> items = day.Value;
                items.Sort((a, b) => a.getStartTime().CompareTo(b.getStartTime()));

                for (int i = 0; i < items.Count - 1; i++)
                {
                    int gap = GetTimeDiffInMins(items[i].getEndTime(), items[i + 1].getStartTime());
                    if (gap > ((request.largestAllowedGap + 1) * 60))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool PassesNumberOfDaysConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (!request.isNumberOfDaysSelected) return true;
            int cnt = 0;
            for (int i = 0; i < 6; i++)
            {
                //check if itemsperday[i] exists and has content
                if (ItemsPerDay.ContainsKey(i) && ItemsPerDay[i].Count > 0)
                {
                    //Console.WriteLine(i);
                    //foreach(var item in ItemsPerDay[i])
                    //{
                    //    Console.WriteLine(item.ToString());
                    //}
                    cnt++;
                }
            }
            return cnt <= request.numberOfDays;
        }

        private static bool PassesSpecificDaysConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (request.isNumberOfDaysSelected) return true;
            for (int i = 0; i < 6; i++)
            {
                if (ItemsPerDay.ContainsKey(i) && ItemsPerDay[i].Count > 0 && !request.selectedDays[i]) return false;
            }
            return true;
        }
        //TODO: Test
        private static bool PassesDayStartConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (string.IsNullOrEmpty(request.daysStart)) return true;

            var dayStart = TimeSpan.Parse(request.daysStart);

            foreach (var day in ItemsPerDay)
            {
                var items = day.Value;
                if (items.Count == 0) continue;

                // Sort in-place once
                items.Sort((a, b) => a.getStartTime().CompareTo(b.getStartTime()));

                // Check if the earliest item's start time is earlier than the allowed start time
                if (items[0].getStartTime() < dayStart)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool PassesDayEndConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (string.IsNullOrEmpty(request.daysEnd)) return true;

            var dayEnd = TimeSpan.Parse(request.daysEnd);

            foreach (var day in ItemsPerDay)
            {
                var items = day.Value;
                if (items.Count == 0) continue;

                // Sort in place
                items.Sort((a, b) => b.getEndTime().CompareTo(a.getEndTime()));

                if (items[0].getEndTime() > dayEnd)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool PassesNumberOfItemsPerDayConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            // If no minimum is specified, always return true
            if (request.minimumNumberOfItemsPerDay == 0) return true;

            foreach (var day in ItemsPerDay)
            {
                // Filter out null or empty lists
                var items = day.Value.Where(item => item != null).ToList();

                // If there are any items on that day, check the count against the minimum
                if (items.Count > 0 && items.Count < request.minimumNumberOfItemsPerDay)
                {
                    return false;  // Not enough items for this day
                }
            }

            return true;
        }


        public static Dictionary<int, List<CardItem>> ConstructItemsPerDay(List<CardItem> currentTimetable)
        {
            Dictionary<int, List<CardItem>> itemsPerDay = new Dictionary<int, List<CardItem>>(6); 
            List<string> days = new List<string> { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };

            foreach (var item in currentTimetable)
            {
                if (item.Schedule.Count == 0) continue;

                var dayOfWeek = days.IndexOf(item.Schedule[0].DayOfWeek);

                if (!itemsPerDay.TryGetValue(dayOfWeek, out var dayItems))
                {
                    dayItems = new List<CardItem>();
                    itemsPerDay[dayOfWeek] = dayItems;
                }

                dayItems.Add(item);
            }
            return itemsPerDay;
        }

        public static bool CardItemPassesConstraints(CardItem item, GenerateRequest request, List<CardItem> currentTimetable)
        {
            // return true;
            List<CardItem> currentTimetableCopy = new List<CardItem>(currentTimetable)
            {
                item
            };
            var itemsPerDay = ConstructItemsPerDay(currentTimetableCopy);
            return PassesNumberOfDaysConstraint(request, itemsPerDay) &&
                PassesNumberOfItemsPerDayConstraint(request, itemsPerDay) &&
                PassesTimeGapConstraint(request, itemsPerDay) &&
                PassesDayStartConstraint(request, itemsPerDay) &&
                PassesSpecificDaysConstraint(request, itemsPerDay) &&
                PassesDayEndConstraint(request, itemsPerDay);

        }

        public static void GenerateTimetablesHelper(List<List<CardItem>> courses, int currentIndex,
    List<CardItem> currentTimetable, List<List<ReturnedCardItem>> timetables, GenerateRequest request)
        {
            // Base case: Max number of generated schedules reached
            if (timetables.Count >= request.maxNumberOfGeneratedSchedules) return;

            // Base case: All courses registered
            if (currentIndex == courses.Count)
            {
                var returnedTimetable = currentTimetable.Select(i => new ReturnedCardItem(i)).ToList();

                // Ensure the current timetable is not a duplicate before adding
                if (!timetables.Any(x => x.SequenceEqual(returnedTimetable)))
                {
                    var itemsPerDay = ConstructItemsPerDay(currentTimetable);

                    // Chain constraints to short-circuit if any fail
                    if (PassesNumberOfDaysConstraint(request, itemsPerDay) &&
                        PassesNumberOfItemsPerDayConstraint(request, itemsPerDay) &&
                        PassesTimeGapConstraint(request, itemsPerDay) &&
                        PassesDayStartConstraint(request, itemsPerDay) &&
                        PassesSpecificDaysConstraint(request, itemsPerDay) &&
                        PassesDayEndConstraint(request, itemsPerDay))
                    {
                        timetables.Add(returnedTimetable);
                    }
                }
                return;
            }

            List<CardItem> currentCourse = courses[currentIndex];
            foreach (var mainSection in currentCourse)
            {
                System.Console.WriteLine("Processing " + mainSection.ToString());
                if (!mainSection.isMainSection()) continue;  // Only process main sections

                if (mainSection.Schedule.Count == 0)  // No schedule
                {
                    System.Console.WriteLine("No schedule for " + mainSection.ToString());
                    if (CardItemPassesConstraints(mainSection, request, currentTimetable))
                    {
                        currentTimetable.Add(mainSection);
                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                        currentTimetable.Remove(mainSection);
                    }
                }
                else if (isCompatible(currentTimetable, mainSection))  // Schedule exists and is compatible
                {
                    if (HasLabAndTutorial(currentCourse))  // Handle multiple subsections
                    {
                        System.Console.WriteLine("Has lab and tutorial for " + mainSection.ToString());
                        if (request.isEngineering)
                        {
                            HandleLabAndTutorialsEngineering(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                        }
                        else HandleLabAndTutorials(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                    }
                    else if (mainSection.hasMultipleSchedules())  // Handle multiple schedules
                    {
                        System.Console.WriteLine("Has multiple schedules for " + mainSection.ToString());
                        HandleMultipleSchedules(mainSection, currentTimetable, courses, currentIndex, timetables, request);
                    }
                    else if (!HasSubsections(currentCourse))  // Electives with no subsections
                    {
                        System.Console.WriteLine("No subsections for " + mainSection.ToString());
                        currentTimetable.Add(mainSection);
                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                        currentTimetable.Remove(mainSection);
                    }
                    else  // Handle one subsection per main section
                    {
                        System.Console.WriteLine("One subsection for " + mainSection.ToString());
                        HandleMainAndSubSections(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                    }
                }
            }
        }

        private static void HandleLabAndTutorials(List<CardItem> currentTimetable, List<CardItem> currentCourse,
    CardItem mainSection, int currentIndex, List<List<ReturnedCardItem>> timetables, GenerateRequest request, List<List<CardItem>> courses)
        {
            Dictionary<string, List<CardItem>> common = new Dictionary<string, List<CardItem>>();
            string mainSectionName = mainSection.Section;
            foreach (var item in currentCourse.Where(i => !i.isMainSection() && isCompatible(currentTimetable, i) && i.Section.StartsWith(mainSectionName)))
            {
                // Check if the key exists and ensure it is not added again
                string section = item.Section.Substring(0, 3);

                if (!common.ContainsKey(section))
                {
                    common[section] = new List<CardItem>();
                }

                // Add only if the section is not already present in the list
                if (!common[section].Contains(item))
                {
                    common[section].Add(item);
                }
            }

            foreach (var entry in common)
            {
                currentTimetable.Add(mainSection);
                currentTimetable.AddRange(entry.Value);
                GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                currentTimetable.RemoveAll(entry.Value.Contains);
                currentTimetable.Remove(mainSection);
            }
        }


        private static void HandleLabAndTutorialsEngineering(List<CardItem> currentTimetable, List<CardItem> currentCourse,
    CardItem mainSection, int currentIndex, List<List<ReturnedCardItem>> timetables, GenerateRequest request, List<List<CardItem>> courses)
        {
            List<CardItem> labs = new List<CardItem>();
            List<CardItem> tutorials = new List<CardItem>();
            string mainSectionName = mainSection.Section;
            foreach (var item in currentCourse.Where(i => !i.isMainSection() && isCompatible(currentTimetable, i) && i.Section.StartsWith(mainSectionName)))
            {
                if (item.SubType.ToLower() == "lab")
                {
                    labs.Add(item);
                }
                else if (item.SubType.ToLower() == "tutorial")
                {
                    tutorials.Add(item);
                }

            }

            foreach (var entry in labs)
            {
                foreach (var entry2 in tutorials)
                {
                    if (CardItemPassesConstraints(entry, request, currentTimetable) && CardItemPassesConstraints(entry2, request, currentTimetable))
                    {
                        currentTimetable.Add(mainSection);
                        currentTimetable.Add(entry);
                        currentTimetable.Add(entry2);
                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                        currentTimetable.Remove(entry2);
                        currentTimetable.Remove(entry);
                        currentTimetable.Remove(mainSection);
                    }

                }
            }
        }


        private static void HandleMultipleSchedules(CardItem mainSection, List<CardItem> currentTimetable,
            List<List<CardItem>> courses, int currentIndex, List<List<ReturnedCardItem>> timetables, GenerateRequest request)
        {
            List<CardItem> mainCourse = new List<CardItem>();
            foreach (var schedule in mainSection.Schedule)
            {
                var card = new CardItem(mainSection) { Schedule = new List<CourseSchedule> { schedule } };
                System.Console.WriteLine(card.ToString());

                mainCourse.Add(card);
            }
            if(!mainCourse.Any(i => CardItemPassesConstraints(i, request, currentTimetable)))
            {
                return;
            }
            currentTimetable.AddRange(mainCourse);
            GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
            currentTimetable.RemoveAll(mainCourse.Contains);
        }

        private static void HandleMainAndSubSections(List<CardItem> currentTimetable, List<CardItem> currentCourse,
            CardItem mainSection, int currentIndex, List<List<ReturnedCardItem>> timetables, GenerateRequest request, List<List<CardItem>> courses)
        {
            System.Console.WriteLine("Handling main and subsections for " + mainSection.ToString());
            foreach (var subSection in currentCourse.Where(sub => !sub.isMainSection() && sub.Section.StartsWith(mainSection.Section) && isCompatible(currentTimetable, sub)))
            {
                List<CardItem> subSections = new List<CardItem>();
                foreach (var schedule in subSection.Schedule)
                {
                    var card = new CardItem(subSection) { Schedule = new List<CourseSchedule> { schedule } };
                    System.Console.WriteLine("subsection: " + card.ToString());
                    subSections.Add(card);
                }
                if(!subSections.Any(i => CardItemPassesConstraints(i, request, currentTimetable)))
                {
                    // currentTimetable.Remove(mainSection);
                    continue;
                }
                currentTimetable.Add(mainSection);
                currentTimetable.AddRange(subSections);
                GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                currentTimetable.RemoveAll(subSections.Contains);
                currentTimetable.Remove(mainSection);
            }
        }

        public static List<List<ReturnedCardItem>> GenerateAllTimetables(List<List<CardItem>> allCardItemsByCourse, GenerateRequest request)
        {
            List<List<ReturnedCardItem>> result = new List<List<ReturnedCardItem>>();
            GenerateTimetablesHelper(allCardItemsByCourse, 0, new List<CardItem>(), result, request);
            return result;

            // // Define the number of days and the number of hours in a day
            // int numberOfDays = 6; // Saturday to Thursday
            // int hoursInDay = 24;

            // List<List<List<ReturnedCardItem>>> schedules = new List<List<List<ReturnedCardItem>>>();
            // foreach (var schedule in result)
            // {
            //     // Initialize the scheduleArray with a size for each day and each hour
            //     List<List<ReturnedCardItem>> scheduleArray = new List<List<ReturnedCardItem>>();
            //     for (int day = 0; day < numberOfDays; day++)
            //     {
            //         // Initialize each day's list with an empty list for each hour
            //         List<ReturnedCardItem> daySchedule = new List<ReturnedCardItem>();
            //         for (int hour = 0; hour < hoursInDay; hour++)
            //         {
            //             daySchedule.Add(null); // default 
            //         }
            //         scheduleArray.Add(daySchedule);
            //     }

            //     foreach (var item in schedule)
            //     {
            //         int dayIndex = Array.IndexOf(new string[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" }, item.day);
            //         if (dayIndex < 0 || dayIndex >= numberOfDays)
            //         {
            //             // dont place it
            //             continue;
            //         }

            //         for (int i = item.startTime.Hours; i < item.endTime.Hours; i++)
            //         {
            //             if (i >= hoursInDay)
            //             {
            //                 throw new ArgumentException("Start time or end time is out of range.");
            //             }
            //             scheduleArray[dayIndex][i] = item;
            //             System.Console.WriteLine(item.ToString());
            //         }
            //         System.Console.WriteLine();
            //     }

            //     schedules.Add(scheduleArray);
            // }
            // return schedules;
        }

        public static bool isCompatible(List<CardItem> currSchedule, CardItem item)
        {
            foreach (var currItem in currSchedule)
            {
                if (currItem.conflictsWith(item))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
