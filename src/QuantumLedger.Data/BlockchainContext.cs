using Microsoft.EntityFrameworkCore;

namespace QuantumLedger.Data;

public class BlockchainContext : DbContext
{
    public BlockchainContext(DbContextOptions<BlockchainContext> options) : base(options) { }
    // Define DbSets for your entities
}