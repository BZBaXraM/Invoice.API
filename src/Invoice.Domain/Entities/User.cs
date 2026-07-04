namespace Invoice.Domain.Entities;

public class User : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public string? Address { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpireTime { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationCode { get; set; }
    public DateTime? EmailConfirmationCodeExpireTime { get; set; }
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpireTime { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
}
