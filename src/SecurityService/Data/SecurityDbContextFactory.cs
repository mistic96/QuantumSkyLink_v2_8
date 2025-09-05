using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace SecurityService.Data;

public class SecurityDbContextFactory : AspireDbContextFactory<SecurityDbContext>
{
    protected override string ConnectionStringName => "postgres-securityservice";

    protected override SecurityDbContext CreateDbContext(DbContextOptions<SecurityDbContext> options)
    {
        return new SecurityDbContext(options);
    }
}
