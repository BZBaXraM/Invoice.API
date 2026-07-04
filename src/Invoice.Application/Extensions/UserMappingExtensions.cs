namespace Invoice.Application.Extensions;

public static class UserMappingExtensions
{
    public static UserResponse ToUserResponse(this User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Username = user.Username,
        Email = user.Email,
        Address = user.Address,
        PhoneNumber = user.PhoneNumber,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
