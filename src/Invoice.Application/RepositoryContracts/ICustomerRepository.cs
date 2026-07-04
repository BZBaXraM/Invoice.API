namespace Invoice.Application.RepositoryContracts;

public interface ICustomerRepository
{
    Customer AddCustomer(Customer customer);

    Task<Customer?> GetByIdAsync(Guid id, Guid ownerUserId);

    Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? nameFilter,
        string? sortBy,
        bool sortDescending);

    Task<bool> HasSentInvoicesAsync(Guid customerId);

    void RemoveCustomer(Customer customer);
}
