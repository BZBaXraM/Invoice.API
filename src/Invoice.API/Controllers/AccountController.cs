namespace Invoice.API.Controllers;

/// <summary>
/// Account registration, authentication, and profile management.
/// </summary>
[Route("api/account")]
[ApiController]
public class AccountController(IAccountService accountService, ICurrentUserService currentUserService)
    : ControllerBase
{
    /// <summary>
    /// Registers a new user and sends an email confirmation code.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ResponseModel>> Register([FromBody] RegisterRequest request)
    {
        var result = await accountService.RegisterAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Confirms a user's email using the code sent to it.
    /// </summary>
    [HttpPost("confirm-email-code")]
    public async Task<ActionResult<ResponseModel>> ConfirmEmailCode([FromBody] ConfirmEmailCodeRequest request)
    {
        var result = await accountService.ConfirmEmailAsync(request.Code);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Requests a new email confirmation code.
    /// </summary>
    [HttpPost("request-confirmation-code")]
    public async Task<ActionResult<ResponseModel>> RequestConfirmationCode([FromBody] RequestConfirmationCodeRequest request)
    {
        var result = await accountService.RequestConfirmationCodeAsync(request.Email);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Logs a user in and returns an access/refresh token pair.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ResponseModel<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await accountService.LoginAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Exchanges a refresh token for a new access/refresh token pair.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ResponseModel<LoginResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await accountService.RefreshTokenAsync(request.RefreshToken);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Logs the current user out, blacklisting the access token and clearing the refresh token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ResponseModel>> Logout()
    {
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var result = await accountService.LogoutAsync(accessToken, currentUserService.UserId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Requests a password reset code by email.
    /// </summary>
    [HttpPost("forget-password")]
    public async Task<ActionResult<ResponseModel>> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        var result = await accountService.ForgetPasswordAsync(request.Email);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Resets a password using the code sent by email.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ResponseModel>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await accountService.ResetPasswordAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<ResponseModel<UserResponse>>> GetProfile()
    {
        var result = await accountService.GetProfileAsync(currentUserService.UserId!.Value);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ResponseModel<UserResponse>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await accountService.UpdateProfileAsync(currentUserService.UserId!.Value, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    [Authorize]
    [HttpPut("profile/change-password")]
    public async Task<ActionResult<ResponseModel>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await accountService.ChangePasswordAsync(currentUserService.UserId!.Value, request);
        return StatusCode(result.StatusCode, result);
    }

    private static readonly JsonSerializerOptions ExportJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };

    /// <summary>
    /// Downloads everything belonging to the current user as a JSON file
    /// (profile, company requisites, customers, invoices, payments, history).
    /// </summary>
    [Authorize(Policy = AuthPolicies.NotAdmin)]
    [HttpGet("profile/export")]
    public async Task<IActionResult> ExportMyData([FromServices] IBackupService backupService)
    {
        var result = await backupService.ExportUserDataAsync(currentUserService.UserId!.Value);
        if (result.IsFailed)
        {
            return StatusCode(result.StatusCode, result);
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes(result.Data, ExportJsonOptions);
        return File(bytes, "application/json", $"my-data-{DateTimeOffset.UtcNow:yyyyMMdd-HHmm}.json");
    }

    /// <summary>
    /// Deletes the current user's profile (hard delete).
    /// </summary>
    [Authorize]
    [HttpDelete("profile")]
    public async Task<ActionResult<ResponseModel>> DeleteProfile()
    {
        var result = await accountService.DeleteProfileAsync(currentUserService.UserId!.Value);
        return StatusCode(result.StatusCode, result);
    }
}
