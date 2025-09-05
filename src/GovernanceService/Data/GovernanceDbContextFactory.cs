using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace GovernanceService.Data;

public class GovernanceDbContextFactory : AspireDbContextFactory<GovernanceDbContext>
{
    protected override string ConnectionStringName => "postgres-governanceservice";

    protected override GovernanceDbContext CreateDbContext(DbContextOptions<GovernanceDbContext> options)
    {
        return new GovernanceDbContext(options);
    }
}
