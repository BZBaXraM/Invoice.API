using System.ComponentModel.DataAnnotations;

namespace Invoice.API.DTOs;

public class ChangePasswordRequestDto
{
    public Guid Id { get; set; }
    [DataType(DataType.Password)] public string OldPassword { get; set; } = string.Empty;
    [DataType(DataType.Password)] public string NewPassword { get; set; } = string.Empty;
}