using System.Threading;
using OrchestrationService.Clients;

namespace OrchestrationService.Services;

public partial class WorkflowManager
{
    // Fix for legacy method calls
    private async Task<Dictionary<string, object>> CallLegacyTestGenerateAsync(object request, CancellationToken cancellationToken = default)
    {
        var result = await _internalMultisigClient.TestGenerateAsync(request, cancellationToken);
        if (result is Dictionary<string, object> dict)
            return dict;
        return new Dictionary<string, object> { ["result"] = result };
    }
    
    private async Task CallLegacyPersistAsync(object request, CancellationToken cancellationToken = default)
    {
        await _internalMultisigClient.PersistAsync(request, cancellationToken);
    }
    
    private async Task CallLegacyPublishSetsAsync(object request, CancellationToken cancellationToken = default)
    {
        await _internalMultisigClient.PublishSetsAsync(request, cancellationToken);
    }
    
    private async Task CallLegacyIngestAsync(object request, CancellationToken cancellationToken = default)
    {
        await _internalMultisigClient.IngestAsync(request, cancellationToken);
    }
}
