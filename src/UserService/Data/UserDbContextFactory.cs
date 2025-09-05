using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace UserService.Data;

public class UserDbContextFactory : AspireDbContextFactory<UserDbContext>
{
    protected override string ConnectionStringName => "postgres-userservice";
    
    protected override UserDbContext CreateDbContext(DbContextOptions<UserDbContext> options)
    {
        return new UserDbContext(options);
    }
}
