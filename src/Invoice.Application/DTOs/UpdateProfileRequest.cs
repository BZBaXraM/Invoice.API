namespace Invoice.Application.DTOs;

/// <summary>
/// Request to update the current user's profile.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>The user's first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>The user's last name.</summary>
    public string? LastName { get; set; }

    /// <summary>The user's username.</summary>
    public string? Username { get; set; }

    /// <summary>The user's email address.</summary>
    public string? Email { get; set; }

    /// <summary>The user's postal address.</summary>
    public string? Address { get; set; }

    /// <summary>The user's phone number.</summary>
    public string? PhoneNumber { get; set; }
}