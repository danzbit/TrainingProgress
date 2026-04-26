using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainingService.Domain.Entities;

namespace TrainingService.Infrastructure.Data.Configurations;

public class ExerciseTypeConfiguration : IEntityTypeConfiguration<ExerciseType>
{
    public void Configure(EntityTypeBuilder<ExerciseType> builder)
    {
        builder.HasKey(et => et.Id);

        builder.Property(et => et.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(et => et.Category)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasMany(et => et.Exercises)
            .WithOne(e => e.ExerciseType)
            .HasForeignKey(e => e.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
