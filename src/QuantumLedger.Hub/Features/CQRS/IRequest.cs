namespace QuantumLedger.Hub.Features.CQRS;

/// <summary>
/// Represents a request that returns a response of type TResponse
/// </summary>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Defines a handler for a request
/// </summary>
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
