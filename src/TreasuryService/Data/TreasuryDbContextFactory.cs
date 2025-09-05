using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace TreasuryService.Data;

public class TreasuryDbContextFactory : AspireDbContextFactory<TreasuryDbContext>
{
    protected override string ConnectionStringName => "postgres-treasuryservice";

    protected override TreasuryDbContext CreateDbContext(DbContextOptions<TreasuryDbContext> options)
    {
        return new TreasuryDbContext(options);
    }
}
