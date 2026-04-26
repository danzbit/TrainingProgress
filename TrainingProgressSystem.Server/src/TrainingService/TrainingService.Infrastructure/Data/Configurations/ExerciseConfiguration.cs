using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainingService.Domain.Entities;

namespace TrainingService.Infrastructure.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.WorkoutId)
            .IsRequired();

        builder.Property(e => e.ExerciseTypeId)
            .IsRequired();

        builder.Property(e => e.Sets)
            .IsRequired();

        builder.Property(e => e.Reps)
            .IsRequired();

        builder.Property(e => e.WeightKg)
            .HasColumnType("decimal(6,2)");

        builder.Property(e => e.DurationSec);

        builder.HasOne(e => e.Workout)
            .WithMany(w => w.Exercises)
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ExerciseType)
            .WithMany(et => et.Exercises)
            .HasForeignKey(e => e.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
