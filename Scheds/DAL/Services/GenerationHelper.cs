using Microsoft.AspNetCore.Identity.Data;
using Scheds.Model;
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
        //h:mm a diff
        public static int GetAbsTimeDiffInMins(TimeSpan t1, TimeSpan t2)
        {
            return (int)Math.Abs((t1 - t2).TotalMinutes);
        }
        private static bool PassesTimeGapConstraint(GenerateRequest request, Dictionary<string, List<CardItem>> ItemsPerDay)
        {
            if (request.largestAllowedGap == 0)
            {
                return true;
            }
            foreach (var day in ItemsPerDay)
            {
                List<CardItem> items = day.Value;
                items.Sort((a, b) => GetAbsTimeDiffInMins(a.getStartTime(), b.getStartTime()));
                for (int i = 0; i < items.Count - 1; i++)
                {
                    if (GetAbsTimeDiffInMins(items[i].getEndTime(), items[i + 1].getStartTime()) > request.largestAllowedGap)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool PassesNumberOfDaysConstraint(GenerateRequest request, Dictionary<string, List<CardItem>> ItemsPerDay)
        {
            if (request.specificDays) return true;
            HashSet<string> days = new HashSet<string>();
            foreach (var day in ItemsPerDay)
            {
                days.Add(day.Key);
            }
            return days.Count <= request.numberOfDays;
        }
        private static bool PassesSpecificDaysConstraint(GenerateRequest request, Dictionary<string, List<CardItem>> ItemsPerDay)
        {
            if (!request.specificDays) return true;
            HashSet<string> days = new HashSet<string>();
            foreach (var day in ItemsPerDay)
            {
                days.Add(day.Key);
            }
            foreach (var day in request.selectedDays)
            {
                if (!days.Contains(day))
                {
                    return false;
                }
            }
            return true;
        }
        //TODO: Test
        private static bool PassesDayStartConstraint(GenerateRequest request, Dictionary<string, List<CardItem>> ItemsPerDay)
        {
            if (request.daysStart == null || request.daysStart == "") return true;
            foreach (var day in ItemsPerDay)
            {
                foreach (var item in day.Value)
                {
                    if (item == null) continue;
                    var dayStart = TimeSpan.Parse(request.daysStart);
                    if (GetAbsTimeDiffInMins(item.getStartTime(), dayStart) < 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool PassesDayEndConstraint(GenerateRequest request, Dictionary<string, List<CardItem>> ItemsPerDay)
        {
            if (request.daysEnd == null || request.daysEnd == "") return true;
            foreach (var day in ItemsPerDay)
            {
                foreach (var item in day.Value)
                {
                    if (item == null) continue;
                    var dayEnd = TimeSpan.Parse(request.daysEnd);
                    //check if dayEnd is before item end time
                    if (GetAbsTimeDiffInMins(item.getEndTime(), dayEnd) < 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static bool PassesNumberOfItemsPerDayConstraint(GenerateRequest request, Dictionary<string, List<CardItem>> ItemsPerDay)
        {
            if (request.minimumNumberOfItemsPerDay == 0) return true;
            foreach (var day in ItemsPerDay)
            {
                if (day.Value.Count < request.minimumNumberOfItemsPerDay)
                {
                    return false;
                }
            }
            return true;
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
                    // Group by day for customization
                    Dictionary<string, List<CardItem>> itemsPerDay = new Dictionary<string, List<CardItem>>();
                    foreach (var item in currentTimetable)
                    {
                        if (item.Schedule.Count == 0) continue;

                        var dayOfWeek = item.Schedule[0].DayOfWeek; // Assuming Schedule is a List<List<string>> as in your Java code
                        if (!itemsPerDay.ContainsKey(dayOfWeek))
                        {
                            itemsPerDay[dayOfWeek] = new List<CardItem>();
                        }
                        itemsPerDay[dayOfWeek].Add(item);
                    }
                    bool nodc = PassesNumberOfDaysConstraint(request, itemsPerDay);
                    bool nopdc = PassesNumberOfItemsPerDayConstraint(request, itemsPerDay);
                    bool tg = PassesTimeGapConstraint(request, itemsPerDay);
                    bool dsc = PassesDayStartConstraint(request, itemsPerDay);
                    bool sdc = PassesSpecificDaysConstraint(request, itemsPerDay);
                    bool dec = PassesDayEndConstraint(request, itemsPerDay);

                    if (nodc&&nopdc&&tg&&dsc&&sdc&&dec)
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
        public static List<List<ReturnedCardItem>> GenerateAllTimetables(List<List<CardItem>> allCardItemsByCourse, GenerateRequest request)
        {
            List<List<ReturnedCardItem>> result = new List<List<ReturnedCardItem>>();

            GenerateTimetablesHelper(allCardItemsByCourse, 0, new List<CardItem>(), result, request);

            return result;
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
