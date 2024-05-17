using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;

namespace InvoiceManager.API.Services.Interfaces;

/// <summary>
/// Interface for the user service.
/// </summary>
public interface IAsyncUserService
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<User> Register(UserRegisterDto dto);

    /// <summary>
    /// Login a user.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<User> Login(UserLoginDto dto);

    /// <summary>
    /// Get a user by id.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<User> UpdateProfile(int userId, UserUpdateDto dto);

    /// <summary>
    /// Change the password of a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task ChangePassword(int userId, ChangePasswordDto dto);

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task DeleteProfile(int userId);
}