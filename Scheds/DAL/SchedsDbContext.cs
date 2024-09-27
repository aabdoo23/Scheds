using Microsoft.EntityFrameworkCore;
using Scheds.Models;
using Scheds.Models.Forum;

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
        public DbSet<CourseSchedule> Schedules_Fall25_New { get; set; }


        public DbSet<User> Users { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Faculty> Faculties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.UserId)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique(); 

            modelBuilder.Entity<CardItem>()
                .HasKey(c => c.CardId); // Specify the primary key for CardItem

            modelBuilder.Entity<CourseBase>()
                .HasKey(c => c.CourseCode); // If CourseCode is the primary key, configure it here
            modelBuilder.Entity<CourseSchedule>()
                .HasKey(cs => cs.ScheduleId);

            // Define the foreign key relationship
            modelBuilder.Entity<CourseSchedule>()
                .HasOne<CardItem>()  // A CourseSchedule references a CardItem
                .WithMany(c => c.Schedule)  // A CardItem can have many schedules
                .HasForeignKey(cs => cs.CardId)
                .OnDelete(DeleteBehavior.Cascade);  // Optional: Cascade delete behavior

            modelBuilder.Entity<User>()
           .HasOne(u => u.Major)
           .WithMany(m => m.Users)
           .HasForeignKey(u => u.MajorId);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Attachments)
                .WithOne(a => a.Post)
                .HasForeignKey(a => a.PostId);

            base.OnModelCreating(modelBuilder);

            foreach (var item in modelBuilder.Model.GetEntityTypes())
            {
                var p = item.FindPrimaryKey().Properties.FirstOrDefault(i => i.ValueGenerated != Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
                if (p != null)
                {
                    p.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
                }
            }
        }
    }
}
