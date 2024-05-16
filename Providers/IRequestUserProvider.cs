using InvoiceManager.API.Data.Entity;

namespace InvoiceManager.API.Providers;

/// <summary>
/// Interface for providing the current user
/// </summary>
public interface IRequestUserProvider
{
    /// <summary>
    /// Get the current user info
    /// </summary>
    /// <returns></returns>
    UserInfo? GetUserInfo();
}