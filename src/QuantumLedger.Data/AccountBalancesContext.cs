using Microsoft.EntityFrameworkCore;

namespace QuantumLedger.Data;

public class AccountBalancesContext : DbContext
{
    public AccountBalancesContext(DbContextOptions<AccountBalancesContext> options) : base(options) { }
    // Define DbSets for your entities
}