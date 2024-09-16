using Microsoft.EntityFrameworkCore;
using Scheds.Models;

namespace Scheds.DAL
{
    public class SchedsDbContext : DbContext
    {
        public SchedsDbContext(DbContextOptions<SchedsDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<CardItem> Sections_Fall25 { get; set; }
        public DbSet<Instructor> All_Instructors_Fall25 { get; set; }
        public DbSet<CourseBase> CourseBase_Fall25 { get; set; }
        public DbSet<CourseSchedule> Schedules_Fall25 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CardItem>()
                .HasKey(c => c.CardId); // Specify the primary key for CardItem

            modelBuilder.Entity<CourseBase>()
                .HasKey(c => c.CourseCode); // If CourseCode is the primary key, configure it here
            modelBuilder.Entity<CourseSchedule>()
       .HasKey(cs => new { cs.CardId, cs.DayOfWeek, cs.StartTime });

            // Define the foreign key relationship
            modelBuilder.Entity<CourseSchedule>()
                .HasOne<CardItem>()  // A CourseSchedule references a CardItem
                .WithMany(c => c.Schedule)  // A CardItem can have many schedules
                .HasForeignKey(cs => cs.CardId)
                .OnDelete(DeleteBehavior.Cascade);  // Optional: Cascade delete behavior

            // Configure other entities if needed
        }
    }
}
