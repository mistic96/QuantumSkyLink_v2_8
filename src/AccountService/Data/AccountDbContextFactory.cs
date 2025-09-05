using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace AccountService.Data;

public class AccountDbContextFactory : AspireDbContextFactory<AccountDbContext>
{
    protected override string ConnectionStringName => "postgres-accountservice";
    
    protected override AccountDbContext CreateDbContext(DbContextOptions<AccountDbContext> options)
    {
        return new AccountDbContext(options);
    }
}
