namespace Invoice.API.Providers;

public interface IRequestUserProvider
{
    UserInfo? GetUserInfo();
}