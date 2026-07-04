namespace Invoice.Application.DTOs;

/// <summary>
/// Request to register a new user.
/// </summary>
public class RegisterRequest
{
    /// <summary>The user's first name.</summary>
    public required string FirstName { get; set; }

    /// <summary>The user's last name.</summary>
    public required string LastName { get; set; }

    /// <summary>The user's username, used as an alternative login identifier.</summary>
    public required string Username { get; set; }

    /// <summary>The user's email address, used as a login identifier.</summary>
    public required string Email { get; set; }

    /// <summary>The user's postal address.</summary>
    public string? Address { get; set; }

    /// <summary>The user's phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>The user's plaintext password (hashed before storage).</summary>
    public required string Password { get; set; }
}
