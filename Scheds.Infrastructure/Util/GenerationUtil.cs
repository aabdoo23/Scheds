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
                if (items.Count < 2) continue; // No gaps possible with less than 2 items

                items.Sort((a, b) => a.GetStartTime().CompareTo(b.GetStartTime()));

                for (int i = 0; i < items.Count - 1; i++)
                {
                    int gapInMinutes = GetTimeDiffInMins(items[i].GetEndTime(), items[i + 1].GetStartTime());
                    int allowedGapInMinutes = GetAllowedGapInMinutes(request.LargestAllowedGap, items[i].GetEndTime());

                    if (gapInMinutes > allowedGapInMinutes)
                    {
                        Console.WriteLine($"Time gap constraint failed: {gapInMinutes}min gap > {allowedGapInMinutes}min allowed");
                        Console.WriteLine($"Between {items[i].CourseCode} ending at {items[i].GetEndTime()} and {items[i + 1].CourseCode} starting at {items[i + 1].GetStartTime()}");
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Calculate allowed gap based on time of day with smart lunch break consideration
        /// </summary>
        private static int GetAllowedGapInMinutes(int requestedGapHours, TimeSpan endTime)
        {
            // Base gap time (user's preference converted to minutes)
            int baseGapMinutes = requestedGapHours * 60;
            
            // Add extra tolerance during lunch hours (11:30 AM - 2:30 PM)
            if (endTime >= new TimeSpan(11, 30, 0) && endTime <= new TimeSpan(14, 30, 0))
            {
                // Allow 30 minutes extra during lunch time for meal breaks
                baseGapMinutes += 30;
            }
            
            return baseGapMinutes;
        }

        private static bool PassesNumberOfDaysConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (!request.IsNumberOfDaysSelected || request.NumberOfDays == 0) return true;
            
            int totalActiveDays = 0;
            int weekdayCount = 0; // Monday-Thursday (indices 2-5)
            int weekendCount = 0; // Saturday-Sunday (indices 0-1)
            
            for (int i = 0; i < 6; i++)
            {
                if (ItemsPerDay.TryGetValue(i, out List<CardItem>? value) && value.Count > 0)
                {
                    totalActiveDays++;
                    
                    // Count weekdays vs weekends for smart prioritization
                    if (i >= 1 && i <= 5) // Sunday-Wednesday
                        weekdayCount++;
                    else if (i == 0) // Saturday
                        weekendCount++;
                }
            }

            // Smart day selection: prefer weekdays when possible
            if (totalActiveDays <= request.NumberOfDays)
            {
                return true;
            }

            // If exceeding limit, check if we can prefer weekdays
            if (weekdayCount <= request.NumberOfDays)
            {
                // All weekdays fit within limit, prefer this arrangement
                Console.WriteLine($"Smart day selection: Preferring {weekdayCount} weekdays over weekend classes");
                return weekdayCount == totalActiveDays; // Only weekdays should be used
            }

            Console.WriteLine($"Number of days constraint failed: {totalActiveDays} active days > {request.NumberOfDays} allowed");
            return false;
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
                    Console.WriteLine("Day start fail");
                    Console.WriteLine(items[0].GetStartTime()+" " + items[0].Section + " " + items[0].CourseCode+ " originalDayStart: "+dayStart);
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
                    Console.WriteLine("Day end fail");
                    Console.WriteLine(items[0].GetEndTime() + " " + items[0].Section + " " + items[0].CourseCode + " originalDayEnd: " + dayEnd);
                    return false;
                }
            }
            return true;
        }
        private static bool PassesNumberOfItemsPerDayConstraint(GenerateRequestDTO request, Dictionary<int, List<CardItem>> ItemsPerDay)
        {
            if (request.MinimumNumberOfItemsPerDay == 0) return true;

            foreach (var day in ItemsPerDay)
            {
                if (day.Value.Count < request.MinimumNumberOfItemsPerDay && day.Value.Count > 0)
                {
                    Console.WriteLine("Number of items per day fail");
                    Console.WriteLine(day.Value.Count + " " + day.Value[0].Section + " " + day.Value[0].CourseCode + " originalNumberOfItems: "+ request.MinimumNumberOfItemsPerDay);
                    return false;
                }
            }
            return true;
        }
        private static bool PassesZeroSeatsConstraint(GenerateRequestDTO request, CardItem item)
        {
            if(!request.ConsiderZeroSeats) return true;
            return item.SeatsLeft > 0;
        }

        /// <summary>
        /// Fast basic time constraint check without building full schedule
        /// </summary>
        private static bool PassesBasicTimeConstraints(CardItem item, GenerateRequestDTO request)
        {
            if (item.CourseSchedules.Count == 0) return true;

            var schedule = item.CourseSchedules.First();
            
            // Check start time constraint
            if (!string.IsNullOrEmpty(request.DaysStart))
            {
                var dayStart = TimeSpan.Parse(request.DaysStart);
                if (schedule.StartTime < dayStart)
                {
                    return false;
                }
            }

            // Check end time constraint
            if (!string.IsNullOrEmpty(request.DaysEnd))
            {
                var dayEnd = TimeSpan.Parse(request.DaysEnd);
                if (schedule.EndTime > dayEnd)
                {
                    return false;
                }
            }

            // Check specific days constraint (early check)
            if (!request.IsNumberOfDaysSelected && request.SelectedDays != null)
            {
                var dayIndex = GetDayIndex(schedule.DayOfWeek);
                if (dayIndex >= 0 && dayIndex < request.SelectedDays.Count && !request.SelectedDays[dayIndex])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Helper method to get day index from day name
        /// </summary>
        private static int GetDayIndex(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "Saturday" => 0,
                "Sunday" => 1,
                "Monday" => 2,
                "Tuesday" => 3,
                "Wednesday" => 4,
                "Thursday" => 5,
                _ => -1
            };
        }

        public static Dictionary<int, List<CardItem>> ConstructItemsPerDay(List<CardItem> currentTimetable)
        {
            Dictionary<int, List<CardItem>> itemsPerDay = new Dictionary<int, List<CardItem>>(6); // Preallocate for 6 Days
            List<string> Days = ["Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"];

            foreach (var item in currentTimetable)
            {
                if (item.CourseSchedules.Count == 0) continue;

                var dayOfWeek = Days.IndexOf(item.CourseSchedules.First().DayOfWeek);

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
            // Early exit: Check zero seats constraint first (fastest check)
            if (!PassesZeroSeatsConstraint(request, item))
            {
                return false;
            }

            // Early exit: Check time-based constraints before building full schedule
            if (!PassesBasicTimeConstraints(item, request))
            {
                return false;
            }

            // Build temporary schedule for complex constraint checking
            List<CardItem> currentTimetableCopy = new(currentTimetable)
            {
                item
            };
            var itemsPerDay = ConstructItemsPerDay(currentTimetableCopy);
            
            // Chain remaining constraints with short-circuit evaluation
            return PassesNumberOfDaysConstraint(request, itemsPerDay) &&
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
            // Base case: Max number of generated schedules reached (early termination)
            if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

            // Base case: All courses registered
            if (currentIndex == courses.Count)
            {
                var returnedTimetable = currentTimetable.Select(i => new ReturnedCardItemDTO(i)).ToList();

                // Fast duplicate check using hash comparison instead of full SequenceEqual
                if (!ContainsDuplicate(timetables, returnedTimetable))
                {
                    var itemsPerDay = ConstructItemsPerDay(currentTimetable);

                    // Final validation with optimized constraint checking
                    if (PassesFinalConstraints(request, itemsPerDay))
                    {
                        timetables.Add(returnedTimetable);
                        Console.WriteLine($"Generated schedule {timetables.Count}/{request.MaxNumberOfGeneratedSchedules}");
                    }
                }
                return;
            }

            List<CardItem> currentCourse = courses[currentIndex];
            
            // Pre-filter main sections for better performance
            var validMainSections = currentCourse
                .Where(section => section.IsMainSection() && 
                                PassesZeroSeatsConstraint(request, section))
                .ToList();

            foreach (var mainSection in validMainSections)
            {
                // Early termination check
                if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

                if (mainSection.CourseSchedules.Count == 0)  // No schedule
                {
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
                        if (request.IsEngineering)
                        {
                            HandleLabAndTutorialsEngineering(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                        }
                        else 
                        {
                            HandleLabAndTutorials(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                        }
                    }
                    else if (mainSection.HasMultipleSchedules())  // Handle multiple schedules
                    {
                        HandleMultipleSchedules(mainSection, currentTimetable, courses, currentIndex, timetables, request);
                    }
                    else if (!HasSubsections(currentCourse))  // Electives with no subsections
                    {
                        if (CardItemPassesConstraints(mainSection, request, currentTimetable))
                        {
                            currentTimetable.Add(mainSection);
                            GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                            currentTimetable.Remove(mainSection);
                        }
                    }
                    else  // Handle one subsection per main section
                    {
                        HandleMainAndSubSections(currentTimetable, currentCourse, mainSection, currentIndex, timetables, request, courses);
                    }
                }
            }
        }

        /// <summary>
        /// Optimized final constraint validation
        /// </summary>
        private static bool PassesFinalConstraints(GenerateRequestDTO request, Dictionary<int, List<CardItem>> itemsPerDay)
        {
            return PassesNumberOfDaysConstraint(request, itemsPerDay) &&
                   PassesNumberOfItemsPerDayConstraint(request, itemsPerDay) &&
                   PassesTimeGapConstraint(request, itemsPerDay) &&
                   PassesDayStartConstraint(request, itemsPerDay) &&
                   PassesSpecificDaysConstraint(request, itemsPerDay) &&
                   PassesDayEndConstraint(request, itemsPerDay);
        }

        /// <summary>
        /// Fast duplicate detection using hash-based comparison
        /// </summary>
        private static bool ContainsDuplicate(List<List<ReturnedCardItemDTO>> timetables, List<ReturnedCardItemDTO> newTimetable)
        {
            if (timetables.Count == 0) return false;
            
            // Create a hash for quick comparison
            var newHash = string.Join("|", newTimetable.Select(item => $"{item.CourseCode}-{item.Section}").OrderBy(x => x));
            
            foreach (var existingTimetable in timetables)
            {
                var existingHash = string.Join("|", existingTimetable.Select(item => $"{item.CourseCode}-{item.Section}").OrderBy(x => x));
                if (newHash == existingHash)
                {
                    return true;
                }
            }
            
            return false;
        }

        private static void HandleLabAndTutorials(List<CardItem> currentTimetable, List<CardItem> currentCourse,
            CardItem mainSection, int currentIndex, List<List<ReturnedCardItemDTO>> timetables, GenerateRequestDTO request, List<List<CardItem>> courses)
        {
            // Early termination check
            if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

            Dictionary<string, List<CardItem>> common = [];
            string mainSectionName = mainSection.Section;
            
            // Pre-filter compatible items with constraint checking
            var compatibleItems = currentCourse
                .Where(i => !i.IsMainSection() && 
                           i.Section.StartsWith(mainSectionName) &&
                           IsCompatible(currentTimetable, i) &&
                           PassesZeroSeatsConstraint(request, i))
                .ToList();

            foreach (var item in compatibleItems)
            {
                string section = item.Section[..3];

                if (!common.TryGetValue(section, out List<CardItem>? value))
                {
                    value = [];
                    common[section] = value;
                }

                if (!value.Contains(item))
                {
                    value.Add(item);
                }
            }

            foreach (var entry in common)
            {
                // Early termination check
                if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

                // Validate all items in the combination before proceeding
                bool allItemsValid = entry.Value.All(item => CardItemPassesConstraints(item, request, currentTimetable));
                
                if (allItemsValid)
                {
                    currentTimetable.Add(mainSection);
                    currentTimetable.AddRange(entry.Value);
                    GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                    currentTimetable.RemoveAll(entry.Value.Contains);
                    currentTimetable.Remove(mainSection);
                }
            }
        }


        private static void HandleLabAndTutorialsEngineering(List<CardItem> currentTimetable,
                                                             List<CardItem> currentCourse, CardItem mainSection,
                                                             int currentIndex,
                                                             List<List<ReturnedCardItemDTO>> timetables,
                                                             GenerateRequestDTO request, List<List<CardItem>> courses)
        {
            // Early termination check
            if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

            List<CardItem> labs = [];
            List<CardItem> tutorials = [];
            string mainSectionName = mainSection.Section;
            
            // Pre-filter compatible items
            var compatibleItems = currentCourse
                .Where(i => !i.IsMainSection() && 
                           IsCompatible(currentTimetable, i) && 
                           i.Section.StartsWith(mainSectionName) &&
                           PassesZeroSeatsConstraint(request, i))
                .ToList();

            foreach (var item in compatibleItems)
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

            foreach (var lab in labs)
            {
                // Early termination check
                if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

                foreach (var tutorial in tutorials)
                {
                    // Early termination check
                    if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

                    // Pre-validate constraints before deep recursion
                    if (CardItemPassesConstraints(lab, request, currentTimetable) && 
                        CardItemPassesConstraints(tutorial, request, currentTimetable))
                    {
                        currentTimetable.Add(mainSection);
                        currentTimetable.Add(lab);
                        currentTimetable.Add(tutorial);
                        GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                        currentTimetable.Remove(tutorial);
                        currentTimetable.Remove(lab);
                        currentTimetable.Remove(mainSection);
                    }
                }
            }
        }


        private static void HandleMultipleSchedules(CardItem mainSection, List<CardItem> currentTimetable,
            List<List<CardItem>> courses, int currentIndex, List<List<ReturnedCardItemDTO>> timetables, GenerateRequestDTO request)
        {
            // Early termination check
            if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

            List<CardItem> validSchedules = [];
            
            // Pre-filter valid schedules
            foreach (var schedule in mainSection.CourseSchedules)
            {
                var card = CardItem.CopyCardItem(mainSection);
                card.CourseSchedules = [schedule];

                // Early constraint validation
                if (PassesZeroSeatsConstraint(request, card) && 
                    PassesBasicTimeConstraints(card, request) &&
                    CardItemPassesConstraints(card, request, currentTimetable))
                {
                    validSchedules.Add(card);
                }
            }

            if (validSchedules.Count == 0) return;

            currentTimetable.AddRange(validSchedules);
            GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
            currentTimetable.RemoveAll(validSchedules.Contains);
        }

        private static void HandleMainAndSubSections(List<CardItem> currentTimetable, List<CardItem> currentCourse,
            CardItem mainSection, int currentIndex, List<List<ReturnedCardItemDTO>> timetables, GenerateRequestDTO request, List<List<CardItem>> courses)
        {
            // Early termination check
            if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

            // Pre-filter compatible subsections
            var compatibleSubSections = currentCourse
                .Where(sub => !sub.IsMainSection() && 
                             sub.Section.StartsWith(mainSection.Section) && 
                             IsCompatible(currentTimetable, sub) &&
                             PassesZeroSeatsConstraint(request, sub))
                .ToList();

            foreach (var subSection in compatibleSubSections)
            {
                // Early termination check
                if (timetables.Count >= request.MaxNumberOfGeneratedSchedules) return;

                List<CardItem> validSubSections = [];
                
                // Pre-filter valid schedules for this subsection
                foreach (var schedule in subSection.CourseSchedules)
                {
                    var card = CardItem.CopyCardItem(subSection);
                    card.CourseSchedules = [schedule];

                    if (PassesBasicTimeConstraints(card, request) &&
                        CardItemPassesConstraints(card, request, currentTimetable))
                    {
                        validSubSections.Add(card);
                    }
                }

                if (validSubSections.Count == 0) continue;

                currentTimetable.Add(mainSection);
                currentTimetable.AddRange(validSubSections);
                GenerateTimetablesHelper(courses, currentIndex + 1, currentTimetable, timetables, request);
                currentTimetable.RemoveAll(validSubSections.Contains);
                currentTimetable.Remove(mainSection);
            }
        }

        public static List<List<ReturnedCardItemDTO>> GenerateAllTimetables(List<List<CardItem>> allCardItemsByCourse, GenerateRequestDTO request)
        {
            var startTime = DateTime.Now;
            List<List<ReturnedCardItemDTO>> result = [];
            
            Console.WriteLine($"Starting schedule generation for {allCardItemsByCourse.Count} courses");
            Console.WriteLine($"Target: {request.MaxNumberOfGeneratedSchedules} schedules");
            Console.WriteLine($"Constraints: MaxDays={request.NumberOfDays}, MaxGap={request.LargestAllowedGap}h, MinSlots={request.MinimumNumberOfItemsPerDay}");
            
            GenerateTimetablesHelper(allCardItemsByCourse, 0, [], result, request);
            
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            Console.WriteLine($"Generation completed in {duration.TotalMilliseconds:F0}ms");
            Console.WriteLine($"Generated {result.Count} valid schedules out of {request.MaxNumberOfGeneratedSchedules} requested");
            
            return result;
        }

        /// <summary>
        /// Optimized compatibility check with early exit
        /// </summary>
        public static bool IsCompatible(List<CardItem> currSchedule, CardItem item)
        {
            // Early exit for items without schedules
            if (item.CourseSchedules.Count == 0) return true;
            
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
