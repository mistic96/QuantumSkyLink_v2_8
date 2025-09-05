using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Logto.AspNetCore.Authentication;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Authentication;

/// <summary>
/// Extension methods for configuring authentication services
/// </summary>
public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds Logto authentication to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddLogtoAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // // Configure LogtoAuthenticationOptions
        // services.Configure<LogtoAuthenticationOptions>(options =>
        // {
        //     options.Authority = configuration["Logto:Endpoint"] ?? "https://localhost:7001";
        //     options.Audience = configuration["Logto:Audience"] ?? "https://api.quantumskylink.com";
        //     options.Issuer = $"{configuration["Logto:Endpoint"] ?? "https://localhost:7001"}/oidc";
        //     options.ClientId = configuration["Logto:ClientId"] ?? "mobile-api-gateway";
        //     options.ClientSecret = configuration["Logto:ClientSecret"] ?? "development-secret-key";
        //     options.EmailClaimType = "email";
        //     options.RoleClaimType = "role";
        // });

        // var appResource = configuration["Logto:Audience"];
        // var appEndpoint = configuration["Logto:Endpoint"];


        // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddJwtBearer(options =>
        //     {
        //         // Logto's OIDC issuer
        //         options.Authority = $"{appEndpoint}/oidc";
        //         // The audience must match your API resource indicator
        //         options.Audience = appResource;

        //         // (Optional) Explicit validation parameters
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuer = true,
        //             ValidIssuer = $"{appEndpoint}/oidc",
        //             ValidateAudience = true,
        //             ValidAudience = appResource,
        //             ValidateLifetime = true
        //         };

        //     });





        // //// Add authentication
        // //services.AddAuthentication(options =>
        // //{
        // //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        // //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        // //})
        // //.AddJwtBearer(options =>
        // //{
        // //    var logtoOptions = services.BuildServiceProvider().GetRequiredService<IOptions<LogtoAuthenticationOptions>>().Value;

        // //    options.Authority = logtoOptions.Authority;
        // //    options.Audience = logtoOptions.Audience;

        // //    options.TokenValidationParameters = new TokenValidationParameters
        // //    {
        // //        ValidateIssuer = true,
        // //        ValidateAudience = true,
        // //        ValidateLifetime = true,
        // //        ValidateIssuerSigningKey = true,
        // //        ValidIssuer = logtoOptions.Issuer,
        // //        ValidAudience = logtoOptions.Audience,
        // //        ClockSkew = TimeSpan.Zero
        // //    };

        // //    // Handle events
        // //    options.Events = new JwtBearerEvents
        // //    {
        // //        OnAuthenticationFailed = context =>
        // //        {
        // //            // Log authentication failures but don't expose details to client
        // //            context.NoResult();
        // //            context.Response.StatusCode = 401;
        // //            context.Response.ContentType = "application/json";

        // //            var result = System.Text.Json.JsonSerializer.Serialize(new { error = "Authentication failed" });
        // //            return context.Response.WriteAsync(result);
        // //        },
        // //        OnForbidden = context =>
        // //        {
        // //            context.Response.StatusCode = 403;
        // //            context.Response.ContentType = "application/json";

        // //            var result = System.Text.Json.JsonSerializer.Serialize(new { error = "Insufficient permissions" });
        // //            return context.Response.WriteAsync(result);
        // //        }
        // //    };
        // //});

        // return services;


         // Use JWT Authentication (default)
    var jwtConfig = configuration.GetSection(JwtConfiguration.SectionName).Get<JwtConfiguration>();
  
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtConfig.Issuer;
                options.Audience = jwtConfig.Audience;
                options.RequireHttpsMetadata = jwtConfig.RequireHttpsMetadata;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = jwtConfig.ValidateAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    ClockSkew = TimeSpan.FromMinutes(jwtConfig.ClockSkewMinutes)
                };

                // Add JWT authentication events for debugging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError("JWT Authentication failed: {Exception}", context.Exception?.Message);
                        
                        if (context.Exception is SecurityTokenMalformedException)
                        {
                            logger.LogError("JWT Token Malformation Error: {Details}", context.Exception.ToString());
                        }
                        
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("JWT Token validated successfully for user: {User}", 
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var token = context.Token;
                        
                        if (!string.IsNullOrEmpty(token))
                        {
                            logger.LogInformation("JWT Token received. Length: {Length}, Prefix: {Prefix}", 
                                token.Length, 
                                token.Length > 20 ? token.Substring(0, 20) : token);
                        }
                        else
                        {
                            logger.LogWarning("No JWT token received in request");
                        }
                        
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("JWT Authentication challenge triggered. Error: {Error}, ErrorDescription: {ErrorDescription}", 
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });
        return services;
    }
}
