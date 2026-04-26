using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data.Configurations;

public class GoalReminderConfiguration : IEntityTypeConfiguration<GoalReminder>
{
    public void Configure(EntityTypeBuilder<GoalReminder> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);
    }
}