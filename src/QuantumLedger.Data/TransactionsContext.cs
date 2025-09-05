using Microsoft.EntityFrameworkCore;

namespace QuantumLedger.Data;

public class TransactionsContext : DbContext
{
    public TransactionsContext(DbContextOptions<TransactionsContext> options) : base(options) { }
    // Define DbSets for your entities
}