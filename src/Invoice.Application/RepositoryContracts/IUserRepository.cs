namespace Invoice.Application.RepositoryContracts;

public interface IUserRepository
{
    User AddUser(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByConfirmationCodeAsync(string code);
    Task<User?> GetByPasswordResetCodeAsync(string code);
    void RemoveUser(User user);
}
