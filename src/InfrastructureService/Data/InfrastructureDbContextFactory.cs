using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace InfrastructureService.Data;

public class InfrastructureDbContextFactory : AspireDbContextFactory<InfrastructureDbContext>
{
    protected override string ConnectionStringName => "postgres-infrastructureservice";

    protected override InfrastructureDbContext CreateDbContext(DbContextOptions<InfrastructureDbContext> options)
    {
        return new InfrastructureDbContext(options);
    }
}
