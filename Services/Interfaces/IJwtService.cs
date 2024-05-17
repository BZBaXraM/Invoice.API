using System.Security.Claims;

namespace InvoiceManager.API.Services.Interfaces;

/// <summary>
/// Interface for JWT service.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a security token.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="email"></param>
    /// <param name="roles"></param>
    /// <param name="userClaims"></param>
    /// <returns></returns>
    string GenerateSecurityToken(
        string id,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim> userClaims);
}