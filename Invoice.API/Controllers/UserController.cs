using AutoMapper;
using Invoice.API.Data;
using Invoice.API.DTOs;
using Invoice.API.Models;
using Invoice.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(InvoiceContext context, IMapperBase mapper) : ControllerBase
{
    private readonly IAsyncUserService _userService = new UserService(context);

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var users = await _userService.GetUsersAsync(filterOn, filterQuery, sortBy,
            isAscending ?? true,
            pageNumber,
            pageSize);

        return Ok(mapper.Map<List<User>>(users));
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDto requestDto)
    {
        var user = mapper.Map<User>(requestDto);
        user.Password = PasswordHasher.Hash(requestDto.Password);

        var newUser = await _userService.RegisterUserAsync(user);

        return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, mapper.Map<UserDto>(newUser));
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserAsync(Guid id)
    {
        var deletedUser = await _userService.DeleteUserAsync(id);
        return Ok(mapper.Map<UserDto>(deletedUser));
    }


    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> SoftDeleteUserAsync(Guid id)
    {
        var user = await _userService.DeleteUserAsync(id);
        return Ok(mapper.Map<User>(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequestDto requestDto)
    {
        var user = mapper.Map<User>(requestDto);
        var loginUser = await _userService.LoginUserAsync(user);

        return Ok(mapper.Map<UserDto>(loginUser));
    }


    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] UpdateLoginRequestDto requestDto)
    {
        if (id != requestDto.Id)
        {
            return BadRequest("Invalid user id");
        }

        var user = mapper.Map<User>(requestDto);
        var updatedUser = await _userService.UpdateUserAsync(user);

        return Ok(mapper.Map<UserDto>(updatedUser));
    }


    [HttpPatch("{id:guid}/changepassword")]
    public async Task<IActionResult> ChangePasswordAsync(Guid id, [FromBody] ChangePasswordRequestDto requestDto)
    {
        var user = mapper.Map<User>(requestDto);
        var changedPasswordUser = await _userService.ChangePasswordAsync(user, requestDto.NewPassword);

        return Ok(mapper.Map<UserDto>(changedPasswordUser));
    }
}