using AnalyticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalyticsService.Infrastructure.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.WorkoutTypeId)
            .IsRequired();

        builder.Property(w => w.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(w => w.DurationMin)
            .IsRequired();

        builder.Property(w => w.Notes)
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UserId)
            .IsRequired();

        builder.HasOne(w => w.WorkoutType)
            .WithMany(wt => wt.Workouts)
            .HasForeignKey(w => w.WorkoutTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
