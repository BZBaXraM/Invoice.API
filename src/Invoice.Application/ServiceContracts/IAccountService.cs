namespace Invoice.Application.ServiceContracts;

public interface IAccountService
{
    Task<ResponseModel> RegisterAsync(RegisterRequest request);
    Task<ResponseModel<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ResponseModel> ConfirmEmailAsync(string code);
    Task<ResponseModel> RequestConfirmationCodeAsync(string email);
    Task<ResponseModel<LoginResponse>> RefreshTokenAsync(string refreshToken);
    Task<ResponseModel> LogoutAsync(string accessToken, Guid? currentUserId);
    Task<ResponseModel<UserResponse>> UpdateProfileAsync(Guid currentUserId, UpdateProfileRequest request);
    Task<ResponseModel> ForgetPasswordAsync(string email);
    Task<ResponseModel> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ResponseModel> ChangePasswordAsync(Guid currentUserId, ChangePasswordRequest request);
    Task<ResponseModel> DeleteProfileAsync(Guid currentUserId);
}
