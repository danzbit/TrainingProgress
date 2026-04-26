using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Data;
using Shared.Contracts.Idempotency;
using Shared.Infrastructure.Data.Configurations;
using Shared.Infrastructure.Identity;

namespace AuthService.Infrastructure.Data;

public class AuthServiceDbContext(DbContextOptions<AuthServiceDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IDbContext
{
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(IdempotencyRecordConfiguration).Assembly);
    }
}
