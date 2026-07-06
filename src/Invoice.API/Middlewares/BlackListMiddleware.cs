namespace Invoice.API.Middlewares;

public class BlackListMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IBlackListService blackListService)
    {
        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        // The SignalR hub authenticates over query string (no custom headers on the
        // WebSocket handshake), so a blacklisted token must be checked there too.
        if (string.IsNullOrWhiteSpace(token) && context.Request.Path.StartsWithSegments("/hubs"))
        {
            token = context.Request.Query["access_token"].ToString();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            await next(context);
            return;
        }

        if (blackListService.IsTokenBlackListed(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await next(context);
    }
}
