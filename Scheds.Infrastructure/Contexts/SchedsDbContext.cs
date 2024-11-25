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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedsDbContext).Assembly);
            
            modelBuilder.Entity<CourseSchedule>()
                .HasOne<CardItem>() 
                .WithMany(c => c.CourseSchedules)  
                .HasForeignKey(cs => cs.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}