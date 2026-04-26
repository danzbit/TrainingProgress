using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainingService.Domain.Entities;

namespace TrainingService.Infrastructure.Data.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.IconUrl)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.SharedAchievements)
            .WithOne(sa => sa.Achievement)
            .HasForeignKey(sa => sa.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
