using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace MarketplaceService.Data;

public class MarketplaceDbContextFactory : AspireDbContextFactory<MarketplaceDbContext>
{
    protected override string ConnectionStringName => "postgres-marketplaceservice";

    protected override MarketplaceDbContext CreateDbContext(DbContextOptions<MarketplaceDbContext> options)
    {
        return new MarketplaceDbContext(options);
    }
}
