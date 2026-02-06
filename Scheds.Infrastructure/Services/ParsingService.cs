using Newtonsoft.Json.Linq;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Util;

namespace Scheds.Infrastructure.Services
{
    public class ParsingService(IInstructorRepository instructorsRepository) : IParsingService
    {
        private readonly IInstructorRepository _instructorsRepository = instructorsRepository
            ?? throw new ArgumentNullException(nameof(instructorsRepository));
        public async Task<List<CardItem>> ParseSelfServiceResponse(string response)
        {
            List<CardItem> ParsedContent = [];
            var instructorNameCache = new Dictionary<string, string>(StringComparer.Ordinal);
            response = System.Text.RegularExpressions.Regex.Unescape(response);
            if (response.StartsWith('\"') && response.EndsWith('\"'))
                response = response[1..^1];
            try
            {
                // Parse the JSON response
                var jsonObject = JObject.Parse(response);
                var dataObject = jsonObject["data"];
                if (dataObject == null || dataObject["sections"] == null)
                    return ParsedContent; // Return empty list if no sections found

                if (dataObject["sections"] is not JArray sectionsArray) return ParsedContent;

                foreach (var sectionObj in sectionsArray)
                {
                    var section = sectionObj["section"]?.ToString();
                    var seats = sectionObj["seatsLeft"]?.ToObject<int>() ?? 0;
                    var instructors = "";

                    if (sectionObj["instructors"] != null && sectionObj["instructors"] is JArray instructorsJson && instructorsJson.Count > 0)
                    {
                        var names = new List<string>();
                        foreach (var instructor in instructorsJson)
                        {
                            var id = instructor["personId"]?.ToString() ?? "0";
                            if (id == "0") continue;
                            var fullName = instructor["fullName"]?.ToString();
                            if (!instructorNameCache.TryGetValue(id, out var name))
                            {
                                var dbName = await _instructorsRepository.GetInstructorNameById(id);
                                if ((string.IsNullOrWhiteSpace(dbName) || string.Equals(dbName, "Pending", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(fullName))
                                    await _instructorsRepository.UpsertAsync(new Instructor { Id = id, FullName = fullName });
                                name = dbName ?? fullName ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(name))
                                    instructorNameCache[id] = name;
                            }
                            if (!string.IsNullOrWhiteSpace(name))
                                names.Add(name);
                        }
                        instructors = names.Count == 0 ? "Pending" : string.Join(", ", names);
                    }

                    var precredits = sectionObj["credits"]?.ToString() ?? "0.00";
                    var credits = Convert.ToDouble(precredits);

                    string cardId = sectionObj["id"]?.ToString() ?? "0";

                    var schedule = new List<CourseSchedule>();
                    if (sectionObj["schedules"] != null && sectionObj["schedules"] is JArray schedulesJSONArray)
                    {
                        foreach (var scheduleJSON in schedulesJSONArray)
                        {
                            var dayOfWeek = scheduleJSON["dayDesc"]?.ToString();

                            // Parse start time and end time, handle AM/PM format
                            TimeSpan startTime = TimeSpan.Parse("00:00"), endTime = TimeSpan.Parse("00:00");
                            if (DateTime.TryParse(scheduleJSON["startTime"]?.ToString(), out var parsedStartTime))
                            {
                                startTime = parsedStartTime.TimeOfDay;
                            }

                            if (DateTime.TryParse(scheduleJSON["endTime"]?.ToString(), out var parsedEndTime))
                            {
                                endTime = parsedEndTime.TimeOfDay;
                            }

                            var location = scheduleJSON["roomId"]?.ToString();

                            var singleSchedule = new CourseSchedule
                            {
                                CardItemId = cardId,
                                DayOfWeek = dayOfWeek,
                                StartTime = startTime,
                                EndTime = endTime,
                                Location = location
                            };
                            IdGenerationUtil.GenerateCourseScheduleId(singleSchedule);
                            schedule.Add(singleSchedule);
                        }
                    }

                    var courseName = sectionObj["eventName"]?.ToString();
                    var courseCode = sectionObj["eventId"]?.ToString();
                    var subType = sectionObj["eventSubType"]?.ToString();
                    var cardItem = new CardItem
                    {
                        Id = cardId,
                        CourseCode = courseCode,
                        CourseName = courseName,
                        Credits = credits,
                        Instructor = instructors,
                        Section = section,
                        SeatsLeft = seats,
                        SubType = subType,
                        CourseSchedules = schedule,
                        LastUpdate = DateTime.UtcNow
                    };
                    ParsedContent.Add(cardItem);
                }
            }
            catch { }
            return ParsedContent;
        }

    }
}
