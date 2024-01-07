namespace Invoice.API.DTOs;

public class UpdatePasswordRequestDto
{
    /// <summary>
    ///     OldPassword
    /// </summary>
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    ///     NewPassword
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}