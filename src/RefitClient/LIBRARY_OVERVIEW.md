# RefitClient Library Overview

## Purpose

The RefitClient library provides a reusable and configurable way to create type-safe HTTP clients for microservice communication in .NET Aspire applications. It leverages Refit for type-safe API clients and integrates with .NET Aspire's service discovery and resilience features.

## Components

### Interfaces

- **IServiceClient**: A marker interface that service client interfaces should implement. This allows for generic type constraints and registration in the DI container.
- **IApiClient<T>**: A generic interface for making HTTP requests. Provides methods for GET, POST, PUT, and DELETE operations.

### Models

- **RefitClientOptions**: Configuration options for Refit clients, including base URL, timeout, authentication settings, and more.
- **ResilienceOptions**: Configuration options for resilience policies, including retry, circuit breaker, and timeout settings.

### Extensions

- **ServiceCollectionExtensions**: Extension methods for registering Refit clients in the DI container. Provides methods for adding clients with default or custom options.

### Resilience

- **ResiliencePolicies**: Extension methods for configuring resilience policies for HTTP clients. Provides methods for adding standard or custom resilience handlers.

### Implementations

- **DefaultApiClient<T>**: Default implementation of the IApiClient<T> interface. Provides methods for making HTTP requests using HttpClient.

### Examples

- **IExampleServiceClient**: An example service client interface that demonstrates how to define API endpoints using Refit attributes.
- **ExampleItem**: An example data model for demonstration purposes.
- **SampleUsage**: A sample usage of the RefitClient library in a .NET Aspire project.

## Usage Flow

1. Define a service client interface that implements IServiceClient and uses Refit attributes to define API endpoints.
2. Register the service client in the DI container using the AddRefitClient extension method.
3. Inject the service client into your services and use it to make API calls.

## Key Features

- **Type-safe API clients**: Define your API endpoints using Refit attributes for type-safe HTTP requests.
- **Service discovery integration**: Automatically discover and connect to services in your .NET Aspire application.
- **Resilience policies**: Built-in support for retry, circuit breaker, and timeout policies.
- **Configurable**: Customize the behavior of your clients through options.
- **Authentication support**: Built-in support for authentication headers.

## Dependencies

- **Refit**: For type-safe HTTP API clients.
- **Refit.HttpClientFactory**: For integration with HttpClientFactory.
- **Microsoft.Extensions.Http.Resilience**: For resilience policies.
- **Microsoft.Extensions.ServiceDiscovery**: For service discovery.
- **Microsoft.Extensions.DependencyInjection.Abstractions**: For DI container integration.
- **Microsoft.Extensions.Options**: For options pattern support.
- **Microsoft.Extensions.Http**: For HttpClient configuration.

## Project Structure

```
RefitClient/
├── Interfaces/
│   ├── IApiClient.cs
│   └── IServiceClient.cs
├── Models/
│   ├── RefitClientOptions.cs
│   └── ResilienceOptions.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Resilience/
│   └── ResiliencePolicies.cs
├── Implementations/
│   └── DefaultApiClient.cs
└── Examples/
    ├── IExampleServiceClient.cs
    └── SampleUsage.cs
```

## Future Enhancements

- Add support for authentication token providers
- Add support for request/response logging
- Add support for custom serialization options
- Add support for API versioning
- Add support for request/response transformation
