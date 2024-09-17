using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Scheds.Models;

namespace Scheds.DAL.Repositories
{
    public class CardItemRepository
    {
        private readonly SchedsDbContext _context;

        public CardItemRepository(SchedsDbContext context)
        {
            _context = context;
        }

        public async Task<List<CardItem>> GetAllCardItemsAsync()
        {
            return await _context.Sections_Fall25.Include(c => c.Schedule).ToListAsync();
        }

        public async Task<List<CardItem>> GetCardItemsByCourseCodeAsync(string courseCode)
        {
            return await _context.Sections_Fall25.Include(c => c.Schedule)
                .Where(cardItem => cardItem.CourseCode == courseCode)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CardItem> GetCardItemByIdAsync(int id)
        {
            return await _context.Sections_Fall25.Include(c => c.Schedule)
                .Where(cardItem => cardItem.CardId == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
        public async Task AddCardItemAsync(CardItem cardItem)
        {
            _context.Sections_Fall25.Add(cardItem);
            await _context.SaveChangesAsync();
        }

        public void UpdateCardItemAsync(CardItem course)
        {
            var sqlConnectionString = "REMOVED";
            using var sqlConnection = new SqlConnection(sqlConnectionString);
            sqlConnection.Open();
            var sqlCommandText = @"
                MERGE sections_Fall25 AS target
                USING (SELECT @cardId AS cardId) AS source
                ON target.cardId = source.cardId
                WHEN MATCHED THEN
                    UPDATE SET 
                        courseCode = @courseCode, 
                        courseName = @courseName, 
                        instructor = @instructor, 
                        credits = @credits, 
                        section = @section, 
                        seatsLeft = @seatsLeft, 
                        subType = @subType, 
                        lastUpdate = @lastUpdate
                WHEN NOT MATCHED THEN
                    INSERT (cardId, courseCode, courseName, instructor, credits, section, seatsLeft, subType, lastUpdate)
                    VALUES (@cardId, @courseCode, @courseName, @instructor, @credits, @section, @seatsLeft, @subType, @lastUpdate);"
            ;

            using var sqlCommandSections = new SqlCommand(sqlCommandText, sqlConnection);
            sqlCommandSections.Parameters.AddWithValue("@cardId", course.CardId);
            sqlCommandSections.Parameters.AddWithValue("@courseCode", course.CourseCode);
            sqlCommandSections.Parameters.AddWithValue("@courseName", course.CourseName);
            sqlCommandSections.Parameters.AddWithValue("@instructor", course.Instructor);
            sqlCommandSections.Parameters.AddWithValue("@credits", course.Credits);
            sqlCommandSections.Parameters.AddWithValue("@section", course.Section);
            sqlCommandSections.Parameters.AddWithValue("@seatsLeft", course.SeatsLeft);
            sqlCommandSections.Parameters.AddWithValue("@subType", course.SubType);
            sqlCommandSections.Parameters.AddWithValue("@lastUpdate", course.LastUpdate);

            sqlCommandSections.ExecuteNonQuery();
        }



    }
}
