using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainingService.Domain.Entities;

namespace TrainingService.Infrastructure.Data.Configurations;

public class WorkoutTypeConfiguration : IEntityTypeConfiguration<WorkoutType>
{
    public void Configure(EntityTypeBuilder<WorkoutType> builder)
    {
        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(wt => wt.Description)
            .HasMaxLength(255);

        builder.HasMany(wt => wt.Workouts)
            .WithOne(w => w.WorkoutType)
            .HasForeignKey(w => w.WorkoutTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
