# XLiquidusMobileService

XLiquidusMobileService is an API gateway service for the XLiquidus mobile application. It provides a unified API for the mobile application to interact with various backend services.

## Architecture

The service follows a microservice architecture with .NET Aspire. It acts as an API gateway for the mobile application, routing requests to the appropriate backend services.

### Authentication

The service uses Logto for authentication, which provides OAuth 2.0/OIDC protocols and JWT tokens for secure authentication.

## API Compatibility Layer

To ensure compatibility with the existing mobile application, the service includes a compatibility layer that maintains the same API signatures as the old MobileOrchestrator. This allows the mobile application to continue functioning without requiring immediate updates.

### Compatibility Controllers

The compatibility controllers are located in the `Controllers/Compatibility` directory. These controllers maintain the same route patterns and request/response models as the old MobileOrchestrator.

- `AuthCompatibilityController`: Handles authentication-related endpoints from the old MobileOrchestrator.

### Compatibility Services

The compatibility services are located in the `Services/Compatibility` directory. These services provide the implementation for the compatibility controllers, translating between the old and new authentication models.

- `AuthCompatibilityService`: Implements the authentication-related functionality for the compatibility layer.

### Compatibility Models

The compatibility models are located in the `Models/Compatibility` directory. These models match the request and response models used by the old MobileOrchestrator.

- `Auth`: Contains authentication-related models for the compatibility layer.

## Development

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code

### Running the Service

To run the service locally:

```bash
dotnet run
```

### Building the Service

To build the service:

```bash
dotnet build
```

### Testing the Service

To run the tests:

```bash
dotnet test
```

## Deployment

The service is deployed as part of the .NET Aspire application. See the main project documentation for deployment instructions.
