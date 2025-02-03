using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Configurations
{
    public class CourseScheduleConfiguration : IEntityTypeConfiguration<CourseSchedule>
    {
        public void Configure(EntityTypeBuilder<CourseSchedule> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder
                .HasOne(cs => cs.CardItem)
                .WithMany(c => c.CourseSchedules)
                .HasForeignKey(cs => cs.CardItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure the CardItemId column is required
            builder.Property(cs => cs.CardItemId)
                .IsRequired();
        }
    }
}
