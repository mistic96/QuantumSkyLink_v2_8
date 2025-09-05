using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace ComplianceService.Data;

public class ComplianceDbContextFactory : AspireDbContextFactory<ComplianceDbContext>
{
    protected override string ConnectionStringName => "postgres-complianceservice";
    
    protected override ComplianceDbContext CreateDbContext(DbContextOptions<ComplianceDbContext> options)
    {
        return new ComplianceDbContext(options);
    }
}
