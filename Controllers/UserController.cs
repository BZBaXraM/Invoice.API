using InvoiceManager.API.Data.Entity;
using InvoiceManager.API.DTOs;
using InvoiceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.API.Controllers;

/// <summary>
/// Controller for user management
/// </summary>
/// <param name="service"></param>
/// <param name="userManager"></param>
/// <param name="signInManager"></param>
/// <param name="jwtService"></param>
[Route("api/[controller]")]
[ApiController]
public class UserController(
    IAsyncUserService service,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IJwtService jwtService)
    : ControllerBase
{
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] UserRegisterDto dto)
    {
        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return Conflict("User with the same email already exists");
        }

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            RefreshToken = Guid.NewGuid().ToString("N").ToLower()
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return await GenerateToken(user);
    }

    /// <summary>
    /// Login a user
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] UserLoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return NotFound("User not found");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            return BadRequest("Invalid password");
        }

        return await GenerateToken(user);
    }

    /// <summary>
    /// Get user profile
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{userId:int}")]
    public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UserUpdateDto dto)
    {
        var user = await service.UpdateProfile(userId, dto);
        return Ok(user);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{userId:int}/change-password")]
    public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto dto)
    {
        await service.ChangePassword(userId, dto);
        return NoContent();
    }

    /// <summary>
    /// Delete user profile
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> DeleteProfile(int userId)
    {
        await service.DeleteProfile(userId);
        return NoContent();
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        if (user is null)
        {
            return Unauthorized();
        }

        user.RefreshToken = Guid.NewGuid().ToString("N").ToLower();
        await userManager.UpdateAsync(user);

        return await GenerateToken(user);
    }

    private async Task<LoginResponseDto> GenerateToken(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var userClaims = await userManager.GetClaimsAsync(user);

        var accessToken = jwtService.GenerateSecurityToken(user.Id, user.UserName!, roles, userClaims);
        var refreshToken = user.RefreshToken;

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken!
        };
    }
}