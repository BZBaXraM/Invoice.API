using System.Security.Claims;

namespace Invoice.API.Providers;

public class RequestUserProvider : IRequestUserProvider
{
    private readonly HttpContext? _context;

    /// <summary>
    /// This constructor is used to define the RequestUserProvider.
    /// </summary>
    /// <param name="context"></param>
    public RequestUserProvider(IHttpContextAccessor context)
        => _context = context.HttpContext;


    /// <summary>
    ///  This method is used to get the UserInfo.
    /// </summary>
    /// <returns></returns>
    public UserInfo? GetUserInfo()
    {
        if (!_context!.User.Claims.Any()) return null;

        var userId = _context.User.Claims.First(c => c.Type == "userId").Value;
        var username = _context.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;

        return new UserInfo(userId, username);
    }
}