using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Contracts.Idempotency;
using System.Text.Json;

namespace Shared.Infrastructure.Data.Configurations;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.HasKey(ir => ir.IdempotencyKey);

        builder.Property(ir => ir.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(ir => ir.Method)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(ir => ir.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ir => ir.StatusCode)
            .IsRequired();

        builder.Property(ir => ir.ResponseBody)
            .IsRequired()
            .HasMaxLength(int.MaxValue);

        builder.Property(ir => ir.Headers)
            .IsRequired()
            .HasMaxLength(int.MaxValue)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions()) ?? new Dictionary<string, string>());

        builder.Property(ir => ir.CreatedAt)
            .IsRequired();
    }
}
