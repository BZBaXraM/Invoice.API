namespace Invoice.API.Middlewares;

public class BlackListMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IBlackListService blackListService)
    {
        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

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
