namespace Invoice.Application.DTOs;

/// <summary>
/// Public representation of a user.
/// </summary>
public class UserResponse
{
    /// <summary>The user's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The user's first name.</summary>
    public required string FirstName { get; set; }

    /// <summary>The user's last name.</summary>
    public required string LastName { get; set; }

    /// <summary>The user's username.</summary>
    public required string Username { get; set; }

    /// <summary>The user's email address.</summary>
    public required string Email { get; set; }

    /// <summary>The user's postal address.</summary>
    public string? Address { get; set; }

    /// <summary>The user's phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>When the user was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the user was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
