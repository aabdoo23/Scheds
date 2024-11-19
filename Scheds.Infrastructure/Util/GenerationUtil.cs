using Scheds.Domain.DTOs;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Util
{
    public static class GenerationUtil
    {
        private static bool HasSubsections(List<CardItem> course)
        {
            foreach (var card in course)
            {
                if (!card.IsMainSection())
                {
                    return true;
                }
            }
            return false;
        }
        private static bool HasLabAndTutorial(List<CardItem> course)
        {
            Dictionary<string, int> freq = [];

            foreach (var card in course)
            {
                if (card.IsMainSection()) continue;
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

        private static bool PassesTimeGapConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (request.LargestAllowedGap == 0) return true;

            foreach (var day in ItemsPerDay)
            {
                List<CardItem> items = day.Value;
                items.Sort((a, b) => a.GetStartTime().CompareTo(b.GetStartTime()));

                for (int i = 0; i < items.Count - 1; i++)
                {
                    int gap = GetTimeDiffInMins(items[i].GetEndTime(), items[i + 1].GetStartTime());
                    if (gap > (request.LargestAllowedGap + 1) * 60)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool PassesNumberOfDaysConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (!request.IsNumberOfDaysSelected) return true;
            int cnt = 0;
            for (int i = 0; i < 6; i++)
            {
                //check if itemsperday[i] exists and has content
                if (ItemsPerDay.TryGetValue(i, out List<CardItem>? value) && value.Count > 0)
                {
                    cnt++;
                }
            }
            return cnt <= request.NumberOfDays;
        }

        private static bool PassesSpecificDaysConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (request.IsNumberOfDaysSelected) return true;
            for (int i = 0; i < 6; i++)
            {
                if (ItemsPerDay.TryGetValue(i, out List<CardItem>? value) && value.Count > 0 && !request.SelectedDays[i]) return false;
            }
            return true;
        }
        //TODO: Test
        private static bool PassesDayStartConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (string.IsNullOrEmpty(request.DaysStart)) return true;

            var dayStart = TimeSpan.Parse(request.DaysStart);

            foreach (var day in ItemsPerDay)
            {
                var items = day.Value;
                if (items.Count == 0) continue;

                // Sort in-place once
                items.Sort((a, b) => a.GetStartTime().CompareTo(b.GetStartTime()));

                // Check if the earliest item's start time is earlier than the allowed start time
                if (items[0].GetStartTime() < dayStart)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool PassesDayEndConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (string.IsNullOrEmpty(request.DaysEnd)) return true;

            var dayEnd = TimeSpan.Parse(request.DaysEnd);

            foreach (var day in ItemsPerDay)
            {
                var items = day.Value;
                if (items.Count == 0) continue;

                // Sort in place
                items.Sort((a, b) => b.GetEndTime().CompareTo(a.GetEndTime()));

                if (items[0].GetEndTime() > dayEnd)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool PassesNumberOfItemsPerDayConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (request.MinimumNumberOfItemsPerDay == 0) return true;

            return ItemsPerDay.All(day => day.Value.Count >= request.MinimumNumberOfItemsPerDay);
        }


        public static Dictionary<int, List<CardItem>> ConstructItemsPerDay(List<CardItem> currentTimetable)
        {
            Dictionary<int, List<CardItem>> itemsPerDay = new Dictionary<int, List<CardItem>>(6); // Preallocate for 6 Days
            List<string> Days = ["Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"];

            foreach (var item in currentTimetable)
            {
                if (item.CourseSchedules.Count == 0) continue;

                var dayOfWeek = Days.IndexOf(item.CourseSchedules[0].DayOfWeek);

                if (!itemsPerDay.TryGetValue(dayOfWeek, out var dayItems))
                {
                    dayItems = [];
                    itemsPerDay[dayOfWeek] = dayItems;
                }

                dayItems.Add(item);
            }
            return itemsPerDay;
        }

        public static bool CardItemPassesConstraints(CardItem item, GenerateRequestDTO request, List<CardItem> currentTimetable)
        {
            // return true;
            List<CardItem> currentTimetableCopy = new(currentTimetable)
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
                                                    List<CardItem> currentTimetable,
                                                    List<List<ReturnedCardItemDTO>> timetables,
                                                    GenerateRequestDTO request)
        {
            // Base case: Max number of generated schedules reached
            if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

            // Base case: All courses registered
            if (currentIndex == courses.Count)
            {
                var returnedTimetable = currentTimetable.Select(i => new ReturnedCardItemDTO(i)).ToList();

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
                Console.WriteLine("Processing " + mainSection.ToString());
                if (!mainSection.IsMainSection()) continue;  // Only process main sections

                if (mainSection.CourseSchedules.Count == 0)  // No schedule
                {
                    Console.WriteLine("No schedule for " + mainSection.ToString());
                    if (CardItemPassesConstraints(mainSection, request, currentTimetable))
                    {
                        currentTimetable.Add(mainSection);
                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                        currentTimetable.Remove(mainSection);
                    }
                }
                else if (IsCompatible(currentTimetable, mainSection))  // Schedule exists and is compatible
                {
                    if (HasLabAndTutorial(currentCourse))  // Handle multiple subsections
                    {
                        Console.WriteLine("Has lab and tutorial for " + mainSection.ToString());
                        if (request.IsEngineering)
                        {
                            HandleLabAndTutorialsEngineering(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                        }
                        else HandleLabAndTutorials(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                    }
                    else if (mainSection.HasMultipleSchedules())  // Handle multiple schedules
                    {
                        Console.WriteLine("Has multiple schedules for " + mainSection.ToString());
                        HandleMultipleSchedules(mainSection, currentTimetable, courses, currentIndex, timetables, request);
                    }
                    else if (!HasSubsections(currentCourse))  // Electives with no subsections
                    {
                        Console.WriteLine("No subsections for " + mainSection.ToString());
                        currentTimetable.Add(mainSection);
                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                        currentTimetable.Remove(mainSection);
                    }
                    else  // Handle one subsection per main section
                    {
                        Console.WriteLine("One subsection for " + mainSection.ToString());
                        HandleMainAndSubSections(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                    }
                }
            }
        }

        private static void HandleLabAndTutorials(List<CardItem> currentTimetable, List<CardItem> currentCourse,
    CardItem mainSection, int currentIndex, List<List<ReturnedCardItemDTO>> timetables, GenerateRequestDTO request, List<List<CardItem>> courses)
        {
            Dictionary<string, List<CardItem>> common = [];
            string mainSectionName = mainSection.Section;
            foreach (var item in currentCourse.Where(i => !i.IsMainSection() && IsCompatible(currentTimetable, i) && i.Section.StartsWith(mainSectionName)))
            {
                // Check if the key exists and ensure it is not added again
                string section = item.Section[..3];

                if (!common.TryGetValue(section, out List<CardItem>? value))
                {
                    value = ([]);
                    common[section] = value;
                }

                // Add only if the section is not already present in the list
                if (!value.Contains(item))
                {
                    value.Add(item);
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


        private static void HandleLabAndTutorialsEngineering(List<CardItem> currentTimetable,
                                                             List<CardItem> currentCourse, CardItem mainSection,
                                                             int currentIndex,
                                                             List<List<ReturnedCardItemDTO>> timetables,
                                                             GenerateRequestDTO request, List<List<CardItem>> courses)
        {
            List<CardItem> labs = [];
            List<CardItem> tutorials = [];
            string mainSectionName = mainSection.Section;
            foreach (var item in currentCourse.Where(i => !i.IsMainSection() && IsCompatible(currentTimetable, i) && i.Section.StartsWith(mainSectionName)))
            {
                if (item.SubType.Equals("lab", StringComparison.CurrentCultureIgnoreCase))
                {
                    labs.Add(item);
                }
                else if (item.SubType.Equals("tutorial", StringComparison.CurrentCultureIgnoreCase))
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
            List<List<CardItem>> courses, int currentIndex, List<List<ReturnedCardItemDTO>> timetables, GenerateRequestDTO request)
        {
            List<CardItem> mainCourse = [];
            foreach (var schedule in mainSection.CourseSchedules)
            {
                var card = CardItem.CopyCardItem(mainSection);
                card.CourseSchedules = [schedule];
                Console.WriteLine(card.ToString());

                mainCourse.Add(card);
            }
            if (!mainCourse.Any(i => CardItemPassesConstraints(i, request, currentTimetable)))
            {
                return;
            }
            currentTimetable.AddRange(mainCourse);
            GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
            currentTimetable.RemoveAll(mainCourse.Contains);
        }

        private static void HandleMainAndSubSections(List<CardItem> currentTimetable, List<CardItem> currentCourse,
            CardItem mainSection, int currentIndex, List<List<ReturnedCardItemDTO>> timetables, GenerateRequestDTO request, List<List<CardItem>> courses)
        {
            Console.WriteLine("Handling main and subsections for " + mainSection.ToString());
            foreach (var subSection in currentCourse.Where(sub => !sub.IsMainSection() && sub.Section.StartsWith(mainSection.Section) && IsCompatible(currentTimetable, sub)))
            {
                List<CardItem> subSections = [];
                foreach (var schedule in subSection.CourseSchedules)
                {
                    var card = CardItem.CopyCardItem(subSection);
                    card.CourseSchedules = [schedule];

                    Console.WriteLine("subsection: " + card.ToString());
                    subSections.Add(card);
                }
                if (!subSections.Any(i => CardItemPassesConstraints(i, request, currentTimetable)))
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

        public static List<List<ReturnedCardItemDTO>> GenerateAllTimetables(List<List<CardItem>> allCardItemsByCourse, GenerateRequestDTO request)
        {
            List<List<ReturnedCardItemDTO>> result = [];
            GenerateTimetablesHelper(allCardItemsByCourse, 0, [], result, request);
            return result;

        }

        public static bool IsCompatible(List<CardItem> currSchedule, CardItem item)
        {
            foreach (var currItem in currSchedule)
            {
                if (currItem.ConflictsWith(item))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
