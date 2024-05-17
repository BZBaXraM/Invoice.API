using InvoiceManager.API.Models;

namespace InvoiceManager.API.DTOs;

/// <summary>
/// Data transfer object for changing password.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Old password.
    /// </summary>
    public string OldPassword { get; set; } = default!;

    /// <summary>
    /// New password.
    /// </summary>
    public string NewPassword { get; set; } = default!;
}