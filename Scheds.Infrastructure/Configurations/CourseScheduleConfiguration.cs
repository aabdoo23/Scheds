using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Configurations
{
    public class CourseScheduleConfiguration : IEntityTypeConfiguration<CourseSchedule>
    {
        public void Configure(EntityTypeBuilder<CourseSchedule> builder)
        {
            builder
                .HasOne<CardItem>()
                .WithMany(ci => ci.CourseSchedules)
                .HasForeignKey(cs => cs.CardItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
