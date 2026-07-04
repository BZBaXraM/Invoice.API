namespace Invoice.Application.Extensions;

public static class CustomerMappingExtensions
{
    public static CustomerResponse ToCustomerResponse(this Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        Email = customer.Email,
        PhoneNumber = customer.PhoneNumber,
        CreatedAt = customer.CreatedAt,
        UpdatedAt = customer.UpdatedAt,
        DeletedAt = customer.DeletedAt
    };
}
