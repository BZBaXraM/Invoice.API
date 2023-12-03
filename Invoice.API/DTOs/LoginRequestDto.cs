using System.ComponentModel.DataAnnotations;

namespace Invoice.API.DTOs;

public class LoginRequestDto
{
    [DataType(DataType.EmailAddress)] public required string Email { get; set; } = string.Empty;
    [DataType(DataType.Password)] public required string Password { get; set; } = string.Empty;
}