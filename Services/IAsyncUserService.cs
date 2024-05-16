using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;

namespace InvoiceManager.API.Services;

public interface IAsyncUserService
{
    Task<User> Register(UserRegisterDto dto);
    Task<User> Login(UserLoginDto dto);
    Task<User> UpdateProfile(int userId, UserUpdateDto dto);
    Task ChangePassword(int userId, ChangePasswordDto dto);
    Task DeleteProfile(int userId);
}