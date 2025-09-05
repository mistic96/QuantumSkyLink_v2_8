using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace UserService.Authentication;

public class LogtoAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public LogtoAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // For now, return no result to allow other authentication schemes to handle
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
