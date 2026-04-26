using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data.Configurations;

public class GoalProgressConfiguration : IEntityTypeConfiguration<GoalProgress>
{
    public void Configure(EntityTypeBuilder<GoalProgress> builder)
    {
        builder.ToTable("GoalProgresses");

        builder.HasKey(p => p.GoalId);

        builder.Property(p => p.CurrentValue)
            .IsRequired();

        builder.Property(p => p.IsCompleted);
    }
}
