using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.DTOs;
using System.Text.RegularExpressions;

namespace Scheds.Infrastructure.Services
{
    public class EmptyRoomsService(ICourseScheduleRepository courseScheduleRepository) : IEmptyRoomsService
    {
        private readonly ICourseScheduleRepository _courseScheduleRepository = courseScheduleRepository
            ?? throw new ArgumentNullException(nameof(courseScheduleRepository));

        public async Task<List<string>> GetEmptyRooms(string dayOfWeek, string time)
        {
            var allRooms = await _courseScheduleRepository.GetAllRoomsAsync();

            var occupiedRooms = await _courseScheduleRepository.GetOccupiedRoomsAtDayTimeAsync(dayOfWeek, time);

            var emptyRooms = allRooms.Except(occupiedRooms).ToList();
            return emptyRooms;
        }

        public async Task<List<RoomAvailabilityDTO>> GetRoomAvailabilityForDay(string dayOfWeek, TimeSpan? currentTime = null, int minimumMinutes = 0)
        {
            // Get all rooms
            var allRooms = await _courseScheduleRepository.GetAllRoomsAsync();

            // Get all schedules for the specified day with course information
            var schedulesForDay = await _courseScheduleRepository.GetSchedulesForDayWithCardItemsAsync(dayOfWeek);

            var roomAvailabilities = new List<RoomAvailabilityDTO>();

            foreach (var room in allRooms)
            {
                var roomSchedules = schedulesForDay
                    .Where(s => s.Location == room)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                var availability = new RoomAvailabilityDTO
                {
                    RoomNumber = room,
                    Building = ExtractBuilding(room),
                    Floor = ExtractFloor(room),
                    BusyPeriods = roomSchedules.Select(s => new TimeBlock
                    {
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        CourseCode = s.CardItem?.CourseCode ?? "Unknown",
                        Section = s.CardItem?.Section ?? ""
                    }).ToList()
                };

                // Calculate free periods
                CalculateFreePeriods(availability, currentTime);

                // If a minimum duration is specified, filter rooms that don't meet it
                if (minimumMinutes > 0 && availability.ContinuousMinutesAvailable < minimumMinutes)
                {
                    continue;
                }

                roomAvailabilities.Add(availability);
            }

            // Sort by building, then floor, then room number
            return roomAvailabilities
                .OrderBy(r => r.Building)
                .ThenBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToList();
        }

        private static void CalculateFreePeriods(RoomAvailabilityDTO availability, TimeSpan? currentTime)
        {
            var dayStart = new TimeSpan(8, 30, 0);  // 8:30 AM
            var dayEnd = new TimeSpan(20, 30, 0);    // 8:30 PM

            var startTime = currentTime ?? dayStart;
            availability.FreePeriods = new List<TimeBlock>();

            if (!availability.BusyPeriods.Any())
            {
                // Room is free all day
                availability.AvailableFrom = startTime;
                availability.AvailableUntil = dayEnd;
                availability.ContinuousMinutesAvailable = (int)(dayEnd - startTime).TotalMinutes;
                availability.FreePeriods.Add(new TimeBlock
                {
                    StartTime = startTime,
                    EndTime = dayEnd
                });
                return;
            }

            var currentPeriodStart = startTime;
            int maxContinuousFree = 0;
            TimeSpan? longestFreeStart = null;
            TimeSpan? longestFreeEnd = null;

            foreach (var busy in availability.BusyPeriods)
            {
                if (busy.StartTime > currentPeriodStart)
                {
                    // There's a free period before this busy period
                    var freeMinutes = (int)(busy.StartTime - currentPeriodStart).TotalMinutes;
                    
                    availability.FreePeriods.Add(new TimeBlock
                    {
                        StartTime = currentPeriodStart,
                        EndTime = busy.StartTime
                    });

                    if (freeMinutes > maxContinuousFree)
                    {
                        maxContinuousFree = freeMinutes;
                        longestFreeStart = currentPeriodStart;
                        longestFreeEnd = busy.StartTime;
                    }
                }

                currentPeriodStart = busy.EndTime > currentPeriodStart ? busy.EndTime : currentPeriodStart;
            }

            // Check if there's a free period after the last busy period
            if (currentPeriodStart < dayEnd)
            {
                var freeMinutes = (int)(dayEnd - currentPeriodStart).TotalMinutes;
                
                availability.FreePeriods.Add(new TimeBlock
                {
                    StartTime = currentPeriodStart,
                    EndTime = dayEnd
                });

                if (freeMinutes > maxContinuousFree)
                {
                    maxContinuousFree = freeMinutes;
                    longestFreeStart = currentPeriodStart;
                    longestFreeEnd = dayEnd;
                }
            }

            availability.ContinuousMinutesAvailable = maxContinuousFree;
            availability.AvailableFrom = longestFreeStart;
            availability.AvailableUntil = longestFreeEnd;
        }

        private static string ExtractBuilding(string roomNumber)
        {
            // Check if it's Tarek Khalil building (starts with digit)
            if (Regex.IsMatch(roomNumber, @"^\d"))
            {
            return "TK"; // Tarek Khalil
            }

            // Check if it's main building (starts with letter)
            if (Regex.IsMatch(roomNumber, @"^[BGFSR]", RegexOptions.IgnoreCase))
            {
            return "Main";
            }

            // Try to extract building code for other formats (e.g., "C5" from "C5-101", "ADMIN" from "ADMIN-201")
            var match = Regex.Match(roomNumber, @"^([A-Z]+\d*|[A-Z]+)[-\s]");
            if (match.Success)
            {
            return match.Groups[1].Value;
            }

            // If it starts with letters, take those
            match = Regex.Match(roomNumber, @"^([A-Z]+)");
            if (match.Success)
            {
            return match.Groups[1].Value;
            }

            return "Unknown";
        }

        private static string ExtractFloor(string roomNumber)
        {
            // Tarek Khalil building: determine floor by room number length and first digit
            if (Regex.IsMatch(roomNumber, @"^\d"))
            {
            // Two digits = basement
            if (roomNumber.Length == 2)
            {
                return "B";
            }
            
            // Three digits: first digit indicates floor
            var firstDigit = roomNumber[0];
            return firstDigit switch
            {
                '1' => "1",
                '2' => "2",
                _ => firstDigit.ToString()
            };
            }

            // Main building: letter indicates floor
            if (Regex.IsMatch(roomNumber, @"^[BGFSR]", RegexOptions.IgnoreCase))
            {
            var firstLetter = char.ToUpper(roomNumber[0]);
            return firstLetter switch
            {
                'G' => "G",
                'B' => "B",
                'F' => "1",
                'S' => "2",
                'R' => "3",
                _ => firstLetter.ToString()
            };
            }

            // Try to extract floor number for other formats
            var match = Regex.Match(roomNumber, @"[-\s]?(\d)");
            if (match.Success)
            {
            return match.Groups[1].Value;
            }

            return "?";
        }
    }
}
