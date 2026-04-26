using AnalyticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalyticsService.Infrastructure.Data.Configurations;

public class WorkoutTypeConfiguration : IEntityTypeConfiguration<WorkoutType>
{
    public void Configure(EntityTypeBuilder<WorkoutType> builder)
    {
        builder.ToTable("WorkoutTypes");

        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(wt => wt.Description)
            .HasColumnType("nvarchar(max)");
    }
}
