using QuantumLedger.Hub.Endpoints;

namespace QuantumLedger.Hub.Extensions
{
    /// <summary>
    /// Extension methods for registering all Minimal API endpoints.
    /// Provides a centralized way to configure all endpoint groups.
    /// </summary>
    public static class EndpointExtensions
    {
        /// <summary>
        /// Maps all QuantumLedger API endpoints to the application.
        /// This method registers all endpoint groups in a centralized manner.
        /// </summary>
        /// <param name="app">The web application builder</param>
        /// <returns>The web application for method chaining</returns>
        public static WebApplication MapQuantumLedgerEndpoints(this WebApplication app)
        {
            // Map all endpoint groups
            app.MapAccountEndpoints();
            app.MapSubstitutionKeyEndpoints(); // Delegation Key System endpoints
            app.MapTransferEndpoints();
            app.MapBalanceEndpoints();
            app.MapTransactionEndpoints();
            app.MapLedgerEndpoints();

            return app;
        }

        /// <summary>
        /// Configures API documentation and OpenAPI settings for all endpoints.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddQuantumLedgerApiDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "QuantumLedger API",
                    Version = "v1",
                    Description = "Multi-cloud account management and blockchain transaction API with post-quantum cryptography support",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "QuantumLedger Team",
                        Email = "support@quantumledger.com"
                    }
                });

                // Add security definitions for signature verification
                options.AddSecurityDefinition("SignatureAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Name = "X-Signature",
                    Description = "Digital signature for request authentication using post-quantum cryptography"
                });

                // Group endpoints by tags
                options.TagActionsBy(api => new[] { api.GroupName ?? "Default" });
                options.DocInclusionPredicate((name, api) => true);
            });

            return services;
        }
    }
}
