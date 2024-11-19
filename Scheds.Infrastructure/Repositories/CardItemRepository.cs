using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class CardItemRepository(SchedsDbContext context) : BaseRepository<CardItem>(context), ICardItemRepository
    {
        private readonly SchedsDbContext _context = context
            ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CardItem>> GetCardItemsByCourseCodeAsync(string courseCode)
        {
            return await _context.CardItems.Include(c => c.CourseSchedules)
                .Where(cardItem => cardItem.CourseCode == courseCode)
                .AsNoTracking()
                .ToListAsync();
        }

        //public void UpdateCardItemAsync(CardItem course)
        //{
        //    var sqlConnectionString = "Server=db7941.public.databaseasp.net; Database=db7941; User Id=db7941; Password=iQ!9N7b?#X3k; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
        //    using var sqlConnection = new SqlConnection(sqlConnectionString);
        //    sqlConnection.Open();
        //    var sqlCommandText = @"
        //        MERGE sections_Fall25 AS target
        //        USING (SELECT @cardId AS cardId) AS source
        //        ON target.cardId = source.cardId
        //        WHEN MATCHED THEN
        //            UPDATE SET 
        //                courseCode = @courseCode, 
        //                courseName = @courseName, 
        //                instructor = @instructor, 
        //                credits = @credits, 
        //                section = @section, 
        //                seatsLeft = @seatsLeft, 
        //                subType = @subType, 
        //                lastUpdate = @lastUpdate
        //        WHEN NOT MATCHED THEN
        //            INSERT (cardId, courseCode, courseName, instructor, credits, section, seatsLeft, subType, lastUpdate)
        //            VALUES (@cardId, @courseCode, @courseName, @instructor, @credits, @section, @seatsLeft, @subType, @lastUpdate);"
        //    ;

        //    using var sqlCommandSections = new SqlCommand(sqlCommandText, sqlConnection);
        //    sqlCommandSections.Parameters.AddWithValue("@cardId", course.CardId);
        //    sqlCommandSections.Parameters.AddWithValue("@courseCode", course.CourseCode);
        //    sqlCommandSections.Parameters.AddWithValue("@courseName", course.CourseName);
        //    sqlCommandSections.Parameters.AddWithValue("@instructor", course.Instructor);
        //    sqlCommandSections.Parameters.AddWithValue("@credits", course.Credits);
        //    sqlCommandSections.Parameters.AddWithValue("@section", course.Section);
        //    sqlCommandSections.Parameters.AddWithValue("@seatsLeft", course.SeatsLeft);
        //    sqlCommandSections.Parameters.AddWithValue("@subType", course.SubType);
        //    sqlCommandSections.Parameters.AddWithValue("@lastUpdate", course.LastUpdate);

        //    sqlCommandSections.ExecuteNonQuery();
        //}



    }
}
