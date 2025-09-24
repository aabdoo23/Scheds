using Microsoft.EntityFrameworkCore;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Contexts
{
    public class SchedsDbContext(DbContextOptions<SchedsDbContext> options) : DbContext(options)
    {
        public DbSet<CardItem> CardItems { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<CourseBase> CourseBases { get; set; }
        public DbSet<CourseSchedule> CourseSchedules { get; set; }
        public DbSet<ScheduleGeneration> ScheduleGenerations { get; set; }
        public DbSet<SelectedCourse> SelectedCourses { get; set; }
        public DbSet<SelectedCustomCourse> SelectedCustomCourses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SeatModeration> SeatModerations { get; set; }
        public DbSet<CartSeatModeration> CartSeatModerations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedsDbContext).Assembly);
            modelBuilder.Entity<CardItem>()
                .HasIndex(c => c.Id)
                .IsUnique();

            modelBuilder.Entity<CourseSchedule>()
                .HasIndex(s => s.Id)
                .IsUnique();

            // Configure relationships for analytics entities
            modelBuilder.Entity<SelectedCourse>()
                .HasOne(sc => sc.ScheduleGeneration)
                .WithMany(sg => sg.SelectedCourses)
                .HasForeignKey(sc => sc.ScheduleGenerationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SelectedCustomCourse>()
                .HasOne(scc => scc.ScheduleGeneration)
                .WithMany(sg => sg.SelectedCustomCourses)
                .HasForeignKey(scc => scc.ScheduleGenerationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SeatModeration entity
            modelBuilder.Entity<SeatModeration>()
                .HasKey(sm => sm.CourseCode_Section);

            // Configure many-to-many relationship between User and SeatModeration
            modelBuilder.Entity<User>()
                .HasMany(u => u.SeatModerations)
                .WithMany(sm => sm.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserSeatModerations",
                    j => j.HasOne<SeatModeration>().WithMany().HasForeignKey("SeatModerationId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"));
                
            base.OnModelCreating(modelBuilder);
        }
    }
}