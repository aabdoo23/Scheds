using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Scheds.DAL.Repositories;
using Scheds.Models;
using System.Text.Json.Nodes;

namespace Scheds.DAL.Services
{
    public class ParsingService
    {
        private readonly AllInstructorsRepository _instructorsRepository;

        public ParsingService(AllInstructorsRepository instructorsRepository)
        {
            _instructorsRepository = instructorsRepository;
        }
        public async Task<List<CardItem>> ParseCourseResponse(string response)
        {
            List<CardItem> ParsedContent = new List<CardItem>();
            response = System.Text.RegularExpressions.Regex.Unescape(response);
            if (response.StartsWith("\"") && response.EndsWith("\""))
            {
                response = response.Substring(1, response.Length - 2);
            }
            //Console.WriteLine(response);
            try
            {
                // Parse the JSON response
                var jsonObject = JObject.Parse(response);
                var dataObject = jsonObject["data"];
                if (dataObject == null || dataObject["sections"] == null)
                    return ParsedContent; // Return empty list if no sections found

                var sectionsArray = dataObject["sections"] as JArray;
                if (sectionsArray == null) return ParsedContent;
                foreach (var sectionObj in sectionsArray)
                {
                    var section = sectionObj["section"]?.ToString();
                    var seats = sectionObj["seatsLeft"]?.ToObject<int>() ?? 0;
                    var instructors = "";

                    if (sectionObj["instructors"] != null && sectionObj["instructors"] is JArray instructorsJson)
                    {
                        foreach (var instructor in instructorsJson)
                        {
                            var id = instructor["personId"]?.ToObject<int>() ?? 0;
                            if (id != 0)
                            {
                                instructors += await _instructorsRepository.GetInstructorNameById(id) + ", ";

                            }
                        }
                        instructors = instructors.TrimEnd(',', ' ');
                    }
                    else
                    {
                        instructors = "Pending";
                    }

                    var precredits = sectionObj["credits"]?.ToString() ?? "0.00";
                    decimal credits = Convert.ToDecimal(precredits);

                    var cardId = sectionObj["id"]?.ToObject<int>() ?? 0;

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
                                CardId=cardId,
                                DayOfWeek = dayOfWeek,
                                StartTime = startTime,
                                EndTime = endTime,
                                Location = location
                            };
                            IdGeneration.GenerateCourseScheduleId(singleSchedule);
                            schedule.Add(singleSchedule);
                        }
                    }

                    var courseName = sectionObj["eventName"]?.ToString();
                    var courseCode = sectionObj["eventId"]?.ToString();
                    var subType = sectionObj["eventSubType"]?.ToString();
                    var lastUpdated = DateTime.Now;


                    var cardItem = new CardItem
                    {
                        CardId = cardId,
                        CourseCode = courseCode,
                        CourseName = courseName,
                        Credits = credits,
                        Instructor = instructors,
                        Section = section,
                        SeatsLeft = seats,
                        SubType = subType,
                        Schedule = schedule,
                        LastUpdate = lastUpdated
                    };
                    Console.WriteLine(cardItem.ToString());

                    ParsedContent.Add(cardItem);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return ParsedContent;
        }

    }
}
