using Newtonsoft.Json.Linq;
using Scheds.DAL.Repositories;
using Scheds.Models;
using System.Text.Json.Nodes;

namespace Scheds.DAL.Services
{
    public static class ParsingService
    {
        public static List<CardItem> ParseCourseResponse(string response)
        {
            List<CardItem> ParsedContent = new List<CardItem>();
            response = System.Text.RegularExpressions.Regex.Unescape(response);
            if (response.StartsWith("\"") && response.EndsWith("\""))
            {
                response = response.Substring(1, response.Length - 2);
            }
            Console.WriteLine(response);
            try
            {
                // Parse the JSON response
                var jsonObject = JObject.Parse(response);
                var dataObject = jsonObject["data"];
                if (dataObject == null || dataObject["sections"] == null)
                    return ParsedContent; // Return empty list if no sections found

                var sectionsArray = dataObject["sections"] as JArray;

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
                            // TODO: Retrieve instructor names from a repository, if available
                            // instructors += instructorsRepository.GetInstructorNameById(id) + ", ";
                        }
                        instructors = instructors.TrimEnd(',', ' ');
                    }
                    else
                    {
                        instructors = "Pending";
                    }

                    var precredits = sectionObj["credits"]?.ToString() ?? "0.00";
                    decimal credits = Convert.ToDecimal(precredits);


                    var schedule = new List<CourseSchedule>();
                    if (sectionObj["schedules"] != null && sectionObj["schedules"] is JArray schedulesJSONArray)
                    {
                        foreach (var scheduleJSON in schedulesJSONArray)
                        {
                            var singleSchedule = new CourseSchedule
                            {
                                DayOfWeek= scheduleJSON["dayDesc"]?.ToString(),   // day
                                StartTime= TimeSpan.Parse(scheduleJSON["startTime"]?.ToString()), // startTime
                                EndTime= TimeSpan.Parse(scheduleJSON["endTime"]?.ToString()),   // endTime
                                Location= scheduleJSON["roomId"]?.ToString()     // room
                            };
                            schedule.Add(singleSchedule);
                        }
                    }

                    var courseName = sectionObj["eventName"]?.ToString();
                    var courseCode = sectionObj["eventId"]?.ToString();
                    var subType = sectionObj["eventSubType"]?.ToString();
                    var lastUpdated = DateTime.Now;

                    var cardId = sectionObj["id"]?.ToObject<int>() ?? 0;

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
