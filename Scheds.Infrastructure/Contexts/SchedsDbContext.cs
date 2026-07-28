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
        public DbSet<SeatAlert> SeatAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedsDbContext).Assembly);
            modelBuilder.Entity<CardItem>()
                .HasIndex(c => c.Id)
                .IsUnique();

            // Every seat check and course lookup filters on CourseCode; without this it scans.
            modelBuilder.Entity<CardItem>()
                .HasIndex(c => c.CourseCode);

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

            // Configure SeatAlert entity (table: SeatAlerts, renamed from SeatModerations)
            modelBuilder.Entity<SeatAlert>()
                .HasKey(sm => sm.Id);

            // User <-> SeatAlert, as an explicit join entity so each subscription can carry its
            // own notification state. The table keeps the original composite key so the migration
            // only renames it and adds a column — existing subscriptions are preserved.
            modelBuilder.Entity<SeatAlertSubscription>(entity =>
            {
                entity.ToTable("UserSeatAlerts");
                entity.HasKey(s => new { s.SeatAlertId, s.UserId });

                entity.HasOne(s => s.SeatAlert)
                    .WithMany(sm => sm.Subscriptions)
                    .HasForeignKey(s => s.SeatAlertId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.User)
                    .WithMany(u => u.SeatSubscriptions)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}