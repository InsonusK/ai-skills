using Microsoft.AspNetCore.Http;
using Middleware.Access;

namespace App.Host.Api.Http.Middleware.Authentication;

/// <summary>
/// The HTTP middleware of the sample access variation point: it identifies
/// the caller. A request with "Authorization: Bearer {token}" of a
/// known token goes on with the token's caller in <see cref="CallerIdentity"/>;
/// a request without the header goes on with none, and the command decides
/// whether it needs one; a header that names no known caller is rejected as
/// unauthenticated at once. Whether the caller may run a command is the
/// command layer's decision (<c>AccessMiddleware</c>), so another API, added
/// later, gets the same check with a middleware of its own.
/// </summary>
public sealed class AuthenticationMiddleware(AccessConfig config) : IMiddleware
{
    private const string Scheme = "Bearer ";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (header == "")
        {
            await next(context);
            return;
        }
        if (!header.StartsWith(Scheme, StringComparison.Ordinal) || !config.Tokens.TryGetValue(header[Scheme.Length..], out var caller))
        {
            throw AccessErrors.UnknownCaller();
        }
        using (CallerIdentity.Begin(caller))
        {
            await next(context);
        }
    }
}
