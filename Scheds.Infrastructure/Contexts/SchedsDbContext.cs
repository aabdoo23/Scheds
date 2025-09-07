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
                
            base.OnModelCreating(modelBuilder);
        }
    }
}