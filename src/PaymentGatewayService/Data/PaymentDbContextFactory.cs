using Microsoft.EntityFrameworkCore;
using QuantunSkyLink_v2.ServiceDefaults;

namespace PaymentGatewayService.Data;

public class PaymentDbContextFactory : AspireDbContextFactory<PaymentDbContext>
{
    protected override string ConnectionStringName => "postgres-paymentgatewayservice";
    
    protected override PaymentDbContext CreateDbContext(DbContextOptions<PaymentDbContext> options)
    {
        return new PaymentDbContext(options);
    }
}
