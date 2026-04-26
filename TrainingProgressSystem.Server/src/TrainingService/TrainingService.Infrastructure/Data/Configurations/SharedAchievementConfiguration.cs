using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainingService.Domain.Entities;

namespace TrainingService.Infrastructure.Data.Configurations;

public class SharedAchievementConfiguration : IEntityTypeConfiguration<SharedAchievement>
{
    public void Configure(EntityTypeBuilder<SharedAchievement> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.AchievementId)
            .IsRequired();

        builder.Property(sa => sa.PublicUrlKey)
            .IsRequired()
            .HasMaxLength(100)
            .HasAnnotation("Unique", true);

        builder.Property(sa => sa.CreatedAt)
            .IsRequired();

        builder.Property(sa => sa.Expiration);

        builder.HasOne(sa => sa.Achievement)
            .WithMany(a => a.SharedAchievements)
            .HasForeignKey(sa => sa.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sa => sa.PublicUrlKey)
            .IsUnique();
    }
}
