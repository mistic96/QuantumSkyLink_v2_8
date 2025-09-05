using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace FeeService.Data;

public class FeeDbContextFactory : AspireDbContextFactory<FeeDbContext>
{
    protected override string ConnectionStringName => "postgres-feeservice";
    
    protected override FeeDbContext CreateDbContext(DbContextOptions<FeeDbContext> options)
    {
        return new FeeDbContext(options);
    }
}
