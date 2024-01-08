using Invoice.API.DTOs;
using Invoice.API.Models;
using Invoice.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Invoice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    ITokenService service)
    : ControllerBase
{
    /// <summary>
    /// Register a user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthTokenDto>> Register([FromBody] RegisterRequestDto request)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Conflict("User with same email already exists");
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            RefreshToken = Guid.NewGuid().ToString("N").ToLower()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return await GenerateToken(user);
    }

    /// <summary>
    /// Login a user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenDto>> Login([FromBody] LoginRequestDto request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized();
        }

        var canSignIn = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!canSignIn.Succeeded)
        {
            return Unauthorized();
        }

        var role = await userManager.GetRolesAsync(user);
        var userClaims = await userManager.GetClaimsAsync(user);

        var accessToken = service.GenerateSecurityToken(user.Id, user.UserName!, role, userClaims);
        var refreshToken = Guid.NewGuid().ToString("N").ToLower();
        user.RefreshToken = refreshToken;
        await userManager.UpdateAsync(user);

        return new AuthTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// Refresh token - get new access token using refresh token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenDto>> Refresh(
        [FromBody] RefreshTokenRequest request)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        if (user is null)
        {
            return Unauthorized();
        }

        return await GenerateToken(user);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<AppUser>> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return user;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AppUser>> UpdateUser(string id, [FromBody] UpdateLoginRequestDto request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.UserName = request.Email;
        user.Email = request.Email;
        user.RefreshToken = Guid.NewGuid().ToString("N").ToLower();

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return user;
    }

    [HttpPut("{id}/password")]
    public async Task<ActionResult<AppUser>> UpdatePassword(string id, [FromBody] UpdatePasswordRequestDto request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return user;
    }


    private async Task<AuthTokenDto> GenerateToken(AppUser user)
    {
        var role = await userManager.GetRolesAsync(user);
        var userClaims = await userManager.GetClaimsAsync(user);

        var accessToken = service.GenerateSecurityToken(user.Id, user.UserName!,
            role, userClaims);
        var refreshToken = user.RefreshToken;

        await userManager.UpdateAsync(user);

        return new AuthTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken!
        };
    }
}