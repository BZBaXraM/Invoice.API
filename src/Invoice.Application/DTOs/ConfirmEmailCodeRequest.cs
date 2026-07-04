namespace Invoice.Application.DTOs;

/// <summary>
/// Request to confirm a user's email using the code sent to it.
/// </summary>
public class ConfirmEmailCodeRequest
{
    /// <summary>The 6-character confirmation code sent to the user's email.</summary>
    public required string Code { get; set; }
}
