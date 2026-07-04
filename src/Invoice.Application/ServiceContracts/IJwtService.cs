namespace Invoice.Application.ServiceContracts;

public interface IJwtService
{
    string GenerateSecurityToken(User user);
    string GenerateRefreshToken();
}
