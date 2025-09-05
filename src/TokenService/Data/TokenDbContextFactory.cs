using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace TokenService.Data;

public class TokenDbContextFactory : AspireDbContextFactory<TokenDbContext>
{
    protected override string ConnectionStringName => "postgres-tokenservice";

    protected override TokenDbContext CreateDbContext(DbContextOptions<TokenDbContext> options)
    {
        return new TokenDbContext(options);
    }
}
