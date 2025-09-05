using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SignatureService.Extensions;

public static class MultisigEndpointExtensions
{
    public static IEndpointRouteBuilder MapMultisigEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Minimal implementation for multisig endpoints
        var group = endpoints.MapGroup("/api/multisig");
        
        group.MapGet("/", () => Results.Ok("Multisig endpoints available"));
        
        return endpoints;
    }
}
