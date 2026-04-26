using AnalyticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalyticsService.Infrastructure.Data.Configurations;

public class AnalyticsSnapshotConfiguration : IEntityTypeConfiguration<AnalyticsSnapshot>
{
    public void Configure(EntityTypeBuilder<AnalyticsSnapshot> builder)
    {
        builder.HasKey(snapshot => snapshot.Id);

        builder.Property(snapshot => snapshot.UserId)
            .IsRequired();

        builder.HasIndex(snapshot => snapshot.UserId)
            .IsUnique();

        builder.Property(snapshot => snapshot.DailyTrendJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(snapshot => snapshot.CountByTypeJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(snapshot => snapshot.LastCalculatedAtUtc)
            .IsRequired();
    }
}
