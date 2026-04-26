using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(g => g.MetricType)
            .IsRequired();

        builder.Property(g => g.PeriodType)
            .IsRequired();

        builder.Property(g => g.TargetValue)
            .IsRequired();

        builder.Property(g => g.Status)
            .IsRequired();

        builder.Property(g => g.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(g => g.EndDate)
            .HasColumnType("date");

        builder.Property(g => g.UserId)
            .IsRequired();

        builder.HasOne(g => g.Progress)
            .WithOne(p => p.Goal)
            .HasForeignKey<GoalProgress>(p => p.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
