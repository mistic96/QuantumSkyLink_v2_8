using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace LiquidationService.Data;

public class LiquidationDbContextFactory : AspireDbContextFactory<LiquidationDbContext>
{
    protected override string ConnectionStringName => "postgres-liquidationservice";

    protected override LiquidationDbContext CreateDbContext(DbContextOptions<LiquidationDbContext> options)
    {
        return new LiquidationDbContext(options);
    }
}
