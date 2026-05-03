using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace HotelListing.Api.Handler;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyValidatorService apiKeyValidator
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string apiKey = string.Empty;

        if(Request.Headers.TryGetValue(AuthenticationDefaults.ApiKeyHeaderName, out var headerValues))
        {
            apiKey = headerValues.ToString();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var valid = await apiKeyValidator.IsValidAsync(apiKey, Context.RequestAborted);
        if (!valid)
        {
            return AuthenticateResult.Fail("Invalid Api Key");
        } 
         
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "apikey"),
            new(ClaimTypes.Name, "ApiKeyClient")

        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);

    }
}
