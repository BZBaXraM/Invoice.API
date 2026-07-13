namespace Invoice.Application.RepositoryContracts;

public interface IPaymentRepository
{
    Payment AddPayment(Payment payment);

    Task<Payment?> GetByIdAsync(Guid id, Guid ownerUserId);

    void RemovePayment(Payment payment);
}
