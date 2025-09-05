using QuantumLedger.Models;
using QuantumLedger.Models.Interfaces;
using QuantumLedger.Data.Storage;

namespace QuantumLedger.Data.Repositories;

/// <summary>
/// Repository for managing Request entities
/// </summary>
public class RequestRepository : BaseRepository<Request>, IRepository<Request>
{
    /// <summary>
    /// Creates a new instance of RequestRepository
    /// </summary>
    /// <param name="dataStore">Data store instance</param>
    public RequestRepository(IDataStore dataStore) 
        : base(dataStore, "requests")
    {
    }

    /// <inheritdoc/>
    protected override string GetEntityId(Request entity) => entity.Id;
}
