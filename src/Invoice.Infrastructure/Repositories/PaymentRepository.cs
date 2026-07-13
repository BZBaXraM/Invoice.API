namespace Invoice.Infrastructure.Repositories;

public class PaymentRepository(InvoiceDbContext context) : IPaymentRepository
{
    public Payment AddPayment(Payment payment)
    {
        context.Payments.Add(payment);
        return payment;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, Guid ownerUserId) =>
        await context.Payments.FirstOrDefaultAsync(p => p.Id == id && p.UserId == ownerUserId);

    public void RemovePayment(Payment payment) => context.Payments.Remove(payment);
}
