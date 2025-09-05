using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace NotificationService.Data;

public class NotificationDbContextFactory : AspireDbContextFactory<NotificationDbContext>
{
    protected override string ConnectionStringName => "postgres-notificationservice";

    protected override NotificationDbContext CreateDbContext(DbContextOptions<NotificationDbContext> options)
    {
        return new NotificationDbContext(options);
    }
}
