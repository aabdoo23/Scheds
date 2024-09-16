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
                freq.Add(card.Section, freq.GetValueOrDefault(card.Section, 0) + 1);
                if (freq[card.Section] > 1)
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
            if (request.largestAllowedGap == 0)
            {
                return true;
            }

            foreach (var day in ItemsPerDay)
            {
                List<CardItem> items = day.Value;

                // Sort based on StartTime directly, not by the difference
                items.Sort((a, b) => a.getStartTime().CompareTo(b.getStartTime()));

                for (int i = 0; i < items.Count - 1; i++)
                {
                    int gap = GetTimeDiffInMins(items[i].getEndTime(), items[i + 1].getStartTime());
                    if (gap > request.largestAllowedGap*60)
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
                if (ItemsPerDay.ContainsKey(i)&&ItemsPerDay[i].Count > 0 && !request.selectedDays[i]) return false;
            }
            return true;
        }
        //TODO: Test
        private static bool PassesDayStartConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            // If no specific start time is given in the request, always return true
            if (string.IsNullOrEmpty(request.daysStart)) return true;

            // Parse the provided start time from the request
            var dayStart = TimeSpan.Parse(request.daysStart);

            foreach (var day in ItemsPerDay)
            {
                // Sort the items by start time to ensure we check the earliest item
                var items = day.Value.Where(item => item != null).OrderBy(item => item.getStartTime()).ToList();
                if (items.Count == 0) continue; // If no items exist for this day, skip it

                // Check if the earliest item's start time is earlier than the allowed start time
                if (items.First().getStartTime() < dayStart)
                {
                    return false;
                }
            }

            return true;
        }


        private static bool PassesDayEndConstraint(GenerateRequest request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            // If no specific end time is given in the request, always return true
            if (string.IsNullOrEmpty(request.daysEnd)) return true;

            // Parse the provided end time from the request
            var dayEnd = TimeSpan.Parse(request.daysEnd);

            foreach (var day in ItemsPerDay)
            {
                // Sort the items by end time to ensure we check the latest item
                var items = day.Value.Where(item => item != null).OrderByDescending(item => item.getEndTime()).ToList();
                if (items.Count == 0) continue; // If no items exist for this day, skip it

                // Check if the latest item's end time is later than the allowed end time
                if (items.First().getEndTime() > dayEnd)
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

        public static Dictionary<int,List<CardItem>> ConstructItemsPerDay(List<CardItem> currentTimetable)
        {
            // Group by day for customization
            Dictionary<int, List<CardItem>> itemsPerDay = new Dictionary<int, List<CardItem>>();
            List<string> days = new List<string> { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };
            foreach (var item in currentTimetable)
            {
                if (item.Schedule.Count == 0) continue;
                var dayOfWeek = days.IndexOf(item.Schedule[0].DayOfWeek);
                if (!itemsPerDay.ContainsKey(dayOfWeek))
                {
                    itemsPerDay[dayOfWeek] = new List<CardItem>();
                }
                itemsPerDay[dayOfWeek].Add(item);
            }
            return itemsPerDay;

        }

        public static void GenerateTimetablesHelper(List<List<CardItem>> courses, int currentIndex,
     List<CardItem> currentTimetable, List<List<ReturnedCardItem>> timetables, GenerateRequest request)
        {
            // Base case: Max number of generated schedules reached
            if (timetables.Count >= request.maxNumberOfGeneratedSchedules) return;

            // Base case: All courses registered
            if (currentIndex == courses.Count)
            {
                // Ensure the current timetable is not a duplicate before adding
                if (!timetables.Any(x => x.SequenceEqual(currentTimetable.Select(i => new ReturnedCardItem(i)))))
                {
                    var itemsPerDay = ConstructItemsPerDay(currentTimetable);

                    bool nodc = PassesNumberOfDaysConstraint(request, itemsPerDay);
                    bool nopdc = PassesNumberOfItemsPerDayConstraint(request, itemsPerDay);
                    bool tg = PassesTimeGapConstraint(request, itemsPerDay);
                    bool dsc = PassesDayStartConstraint(request, itemsPerDay);
                    bool sdc = PassesSpecificDaysConstraint(request, itemsPerDay);
                    bool dec = PassesDayEndConstraint(request, itemsPerDay);
                    Console.WriteLine(nodc + " " + nopdc + " " + tg + " " + dsc + " " + sdc + " " + dec);

                    if (nodc && nopdc && tg && dsc && sdc && dec)
                    {
                        var returnedTimetable = currentTimetable
                            .Select(i => new ReturnedCardItem(i))
                            .ToList();

                        timetables.Add(new List<ReturnedCardItem>(returnedTimetable));
                    }
                }

                return;
            }

            List<CardItem> currentCourse = courses[currentIndex];
            foreach (var mainSection in currentCourse)
            {
                if (mainSection.isMainSection())
                {
                    // No schedule
                    if (mainSection.Schedule.Count == 0)
                    {
                        currentTimetable.Add(mainSection);
                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                        currentTimetable.Remove(mainSection);
                    }
                    // Has a schedule and is compatible
                    else if (isCompatible(currentTimetable, mainSection))
                    {
                        // Case: Multiple subsections considered together (e.g., physics)
                        if (HasLabAndTutorial(currentCourse))
                        {
                            Dictionary<string, List<CardItem>> common = new Dictionary<string, List<CardItem>>();
                            foreach (var i in currentCourse)
                            {
                                if (i.isMainSection()) continue;

                                if (isCompatible(currentTimetable, i))
                                {
                                    if (!common.ContainsKey(i.Section))
                                    {
                                        common[i.Section] = new List<CardItem>();
                                    }
                                    common[i.Section].Add(i);
                                }
                            }

                            foreach (var i in currentCourse)
                            {
                                if (i.isMainSection())
                                {
                                    foreach (var entry in common)
                                    {
                                        if (entry.Key.StartsWith(i.Section))
                                        {
                                            entry.Value.Add(i);
                                        }
                                    }
                                }
                            }

                            foreach (var entry in common)
                            {
                                currentTimetable.AddRange(entry.Value);
                                GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                                currentTimetable.RemoveAll(entry.Value.Contains);
                            }
                        }
                        // Multiple schedules (e.g., English)
                        else if (mainSection.hasMultipleSchedules())
                        {
                            List<CardItem> generatedSchedules = new List<CardItem>();
                            foreach (var schedule in mainSection.Schedule)
                            {
                                var card = new CardItem(mainSection);
                                card.Schedule = new List<CourseSchedule> { schedule };
                                generatedSchedules.Add(card);
                            }

                            currentTimetable.AddRange(generatedSchedules);
                            GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                            currentTimetable.RemoveAll(generatedSchedules.Contains);
                        }
                        // No subsections (e.g., electives)
                        else if (!HasSubsections(currentCourse))
                        {
                            currentTimetable.Add(mainSection);
                            GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                            currentTimetable.Remove(mainSection);
                        }
                        // One subsection per main section
                        else
                        {
                            foreach (var subSection in currentCourse)
                            {
                                if (!subSection.isMainSection() && subSection.Section.StartsWith(mainSection.Section))
                                {
                                    if (isCompatible(currentTimetable, subSection))
                                    {
                                        currentTimetable.Add(mainSection);
                                        currentTimetable.Add(subSection);
                                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                                        currentTimetable.Remove(subSection);
                                        currentTimetable.Remove(mainSection);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static List<List<List<ReturnedCardItem>>> GenerateAllTimetables(List<List<CardItem>> allCardItemsByCourse, GenerateRequest request)
        {
            List<List<ReturnedCardItem>> result = new List<List<ReturnedCardItem>>();

            GenerateTimetablesHelper(allCardItemsByCourse, 0, new List<CardItem>(), result, request);

            // Define the number of days and the number of hours in a day
            int numberOfDays = 6; // Saturday to Thursday
            int hoursInDay = 24;

            List<List<List<ReturnedCardItem>>> schedules = new List<List<List<ReturnedCardItem>>>();
            foreach (var schedule in result)
            {
                // Initialize the scheduleArray with a size for each day and each hour
                List<List<ReturnedCardItem>> scheduleArray = new List<List<ReturnedCardItem>>();
                for (int day = 0; day < numberOfDays; day++)
                {
                    // Initialize each day's list with an empty list for each hour
                    List<ReturnedCardItem> daySchedule = new List<ReturnedCardItem>();
                    for (int hour = 0; hour < hoursInDay; hour++)
                    {
                        daySchedule.Add(null); // default 
                    }
                    scheduleArray.Add(daySchedule);
                }

                foreach (var item in schedule)
                {
                    int dayIndex = Array.IndexOf(new string[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" }, item.day);
                    if (dayIndex < 0 || dayIndex >= numberOfDays)
                    {
                        // dont place it
                        continue;
                    }

                    for (int i = item.startTime.Hours; i < item.endTime.Hours; i++)
                    {
                        if (i >= hoursInDay)
                        {
                            throw new ArgumentException("Start time or end time is out of range.");
                        }
                        scheduleArray[dayIndex][i] = item;
                        System.Console.WriteLine(item.ToString());
                    }
                    System.Console.WriteLine();
                }

                schedules.Add(scheduleArray);
            }
            return schedules;
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
