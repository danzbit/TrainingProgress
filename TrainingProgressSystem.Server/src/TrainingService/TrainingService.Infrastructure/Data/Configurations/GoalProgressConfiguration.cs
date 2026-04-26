using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainingService.Domain.Entities;

namespace TrainingService.Infrastructure.Data.Configurations;

public class GoalProgressConfiguration : IEntityTypeConfiguration<GoalProgress>
{
    public void Configure(EntityTypeBuilder<GoalProgress> builder)
    {
        builder.ToTable("GoalProgresses");

        builder.HasKey(gp => gp.GoalId);

        builder.Property(gp => gp.CurrentValue)
            .IsRequired();

        builder.Property(gp => gp.Percentage)
            .IsRequired();

        builder.Property(gp => gp.IsCompleted)
            .IsRequired();

        builder.Property(gp => gp.LastCalculatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasOne(gp => gp.Goal)
            .WithOne(g => g.Progress)
            .HasForeignKey<GoalProgress>(gp => gp.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
