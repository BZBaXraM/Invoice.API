namespace Invoice.Application.Services;

public class AccountService(
    IUnitOfWork uow,
    IJwtService jwtService,
    IEmailService emailService,
    IBlackListService blackListService,
    RegisterRequestValidator registerValidator,
    LoginRequestValidator loginValidator,
    ConfirmEmailCodeRequestValidator confirmEmailCodeValidator,
    ResetPasswordRequestValidator resetPasswordValidator,
    UpdateProfileRequestValidator updateProfileValidator,
    ChangePasswordRequestValidator changePasswordValidator) : IAccountService
{
    public async Task<ResponseModel> RegisterAsync(RegisterRequest request)
    {
        var validation = await registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (await uow.UserRepository.GetByEmailAsync(request.Email) is not null)
        {
            return ResponseModel.Failure("Email already exists", 409);
        }

        if (await uow.UserRepository.GetByUsernameAsync(request.Username) is not null)
        {
            return ResponseModel.Failure("Username already exists", 409);
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = request.Username,
            Email = request.Email,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Password = PasswordHelper.Hash(request.Password),
            IsEmailConfirmed = false,
            EmailConfirmationCode = CodeHelper.GenerateRandom(),
            EmailConfirmationCodeExpireTime = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        uow.UserRepository.AddUser(user);
        await uow.CommitAsync();

        try
        {
            await emailService.SendConfirmationEmailAsync(user.Email, user.EmailConfirmationCode);
            return ResponseModel.Success("Registration successful. Confirmation code sent to your email.");
        }
        catch (Exception)
        {
            return ResponseModel.Success(
                "Registration successful, but failed to send confirmation email. Please request a new code.");
        }
    }

    public async Task<ResponseModel<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var validation = await loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<LoginResponse>("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await uow.UserRepository.GetByEmailOrUsernameAsync(request.UsernameOrEmail);
        if (user is null || !PasswordHelper.Verify(request.Password, user.Password))
        {
            return ResponseModel.Failure<LoginResponse>("Invalid credentials", 401);
        }

        if (!user.IsEmailConfirmed)
        {
            return ResponseModel.Failure<LoginResponse>("Email not confirmed. Please confirm your email before logging in.", 403);
        }

        IssueTokens(user);
        await uow.CommitAsync();

        return ResponseModel.Success(BuildLoginResponse(user));
    }

    public async Task<ResponseModel> ConfirmEmailAsync(string code)
    {
        var validation = await confirmEmailCodeValidator.ValidateAsync(new ConfirmEmailCodeRequest { Code = code });
        if (!validation.IsValid)
        {
            return ResponseModel.Failure("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await uow.UserRepository.GetByConfirmationCodeAsync(code);
        if (user is null)
        {
            return ResponseModel.Failure("Invalid confirmation code.", 400);
        }

        if (user.IsEmailConfirmed)
        {
            return ResponseModel.Failure("Email is already confirmed.", 400);
        }

        if (user.EmailConfirmationCodeExpireTime is null || user.EmailConfirmationCodeExpireTime < DateTime.UtcNow)
        {
            return ResponseModel.Failure("Confirmation code has expired. Please request a new code.", 400);
        }

        user.IsEmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpireTime = null;

        await uow.CommitAsync();

        return ResponseModel.Success("Email confirmed successfully. You can now log in.");
    }

    public async Task<ResponseModel> RequestConfirmationCodeAsync(string email)
    {
        var user = await uow.UserRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return ResponseModel.Failure("User with this email not found.", 404);
        }

        if (user.IsEmailConfirmed)
        {
            return ResponseModel.Failure("Email is already confirmed.", 400);
        }

        user.EmailConfirmationCode = CodeHelper.GenerateRandom();
        user.EmailConfirmationCodeExpireTime = DateTime.UtcNow.AddMinutes(5);

        await uow.CommitAsync();

        try
        {
            await emailService.SendConfirmationEmailAsync(user.Email, user.EmailConfirmationCode);
            return ResponseModel.Success("Confirmation code sent to your email.");
        }
        catch (Exception)
        {
            return ResponseModel.Failure("Failed to send confirmation email. Please try again later.", 500);
        }
    }

    public async Task<ResponseModel<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        var user = await uow.UserRepository.GetByRefreshTokenAsync(refreshToken);
        if (user is null)
        {
            return ResponseModel.Failure<LoginResponse>("Invalid refresh token", 401);
        }

        if (user.RefreshTokenExpireTime is null || user.RefreshTokenExpireTime < DateTime.UtcNow)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpireTime = null;
            await uow.CommitAsync();
            return ResponseModel.Failure<LoginResponse>("Refresh token has expired", 401);
        }

        IssueTokens(user);
        await uow.CommitAsync();

        return ResponseModel.Success(BuildLoginResponse(user));
    }

    public async Task<ResponseModel> LogoutAsync(string accessToken, Guid? currentUserId)
    {
        blackListService.AddTokenToBlackList(accessToken);

        if (currentUserId is not null)
        {
            var user = await uow.UserRepository.GetByIdAsync(currentUserId.Value);
            if (user is not null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpireTime = null;
                await uow.CommitAsync();
            }
        }

        return ResponseModel.Success();
    }

    public async Task<ResponseModel<UserResponse>> UpdateProfileAsync(Guid currentUserId, UpdateProfileRequest request)
    {
        var validation = await updateProfileValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<UserResponse>("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await uow.UserRepository.GetByIdAsync(currentUserId);
        if (user is null)
        {
            return ResponseModel.Failure<UserResponse>("User not found", 404);
        }

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingByEmail = await uow.UserRepository.GetByEmailAsync(request.Email);
            if (existingByEmail is not null && existingByEmail.Id != user.Id)
            {
                return ResponseModel.Failure<UserResponse>("Email already exists", 409);
            }
        }

        if (!string.Equals(user.Username, request.Username, StringComparison.OrdinalIgnoreCase))
        {
            var existingByUsername = await uow.UserRepository.GetByUsernameAsync(request.Username);
            if (existingByUsername is not null && existingByUsername.Id != user.Id)
            {
                return ResponseModel.Failure<UserResponse>("Username already exists", 409);
            }
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Username = request.Username;
        user.Email = request.Email;
        user.Address = request.Address;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await uow.CommitAsync();

        return ResponseModel.Success(user.ToUserResponse());
    }

    public async Task<ResponseModel> ForgetPasswordAsync(string email)
    {
        var user = await uow.UserRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return ResponseModel.Failure("User with this email not found.", 404);
        }

        user.PasswordResetCode = CodeHelper.GenerateRandom();
        user.PasswordResetCodeExpireTime = DateTime.UtcNow.AddMinutes(15);

        await uow.CommitAsync();

        try
        {
            await emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetCode);
            return ResponseModel.Success("Password reset code sent to your email.");
        }
        catch (Exception)
        {
            return ResponseModel.Failure("Failed to send password reset email. Please try again later.", 500);
        }
    }

    public async Task<ResponseModel> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var validation = await resetPasswordValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await uow.UserRepository.GetByPasswordResetCodeAsync(request.Code);
        if (user is null)
        {
            return ResponseModel.Failure("Invalid reset code.", 400);
        }

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            return ResponseModel.Failure("Email does not match the reset code.", 400);
        }

        if (user.PasswordResetCodeExpireTime is null || user.PasswordResetCodeExpireTime < DateTime.UtcNow)
        {
            return ResponseModel.Failure("Reset code has expired. Please request a new code.", 400);
        }

        user.Password = PasswordHelper.Hash(request.NewPassword);
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpireTime = null;
        user.RefreshToken = null;
        user.RefreshTokenExpireTime = null;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await uow.CommitAsync();

        return ResponseModel.Success("Password reset successfully. You can now log in with your new password.");
    }

    public async Task<ResponseModel> ChangePasswordAsync(Guid currentUserId, ChangePasswordRequest request)
    {
        var validation = await changePasswordValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await uow.UserRepository.GetByIdAsync(currentUserId);
        if (user is null)
        {
            return ResponseModel.Failure("User not found", 404);
        }

        if (!PasswordHelper.Verify(request.OldPassword, user.Password))
        {
            return ResponseModel.Failure("Invalid current password", 401);
        }

        user.Password = PasswordHelper.Hash(request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.RefreshToken = null;
        user.RefreshTokenExpireTime = null;

        await uow.CommitAsync();

        return ResponseModel.Success("Password changed successfully");
    }

    public async Task<ResponseModel> DeleteProfileAsync(Guid currentUserId)
    {
        var user = await uow.UserRepository.GetByIdAsync(currentUserId);
        if (user is null)
        {
            return ResponseModel.Failure("User not found", 404);
        }

        uow.UserRepository.RemoveUser(user);
        await uow.CommitAsync();

        return ResponseModel.Success("Profile deleted successfully");
    }

    private void IssueTokens(User user)
    {
        user.RefreshToken = jwtService.GenerateRefreshToken();
        user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
    }

    private LoginResponse BuildLoginResponse(User user) => new()
    {
        AccessToken = jwtService.GenerateSecurityToken(user),
        RefreshToken = user.RefreshToken!,
        RefreshTokenExpireTime = user.RefreshTokenExpireTime!.Value
    };
}
