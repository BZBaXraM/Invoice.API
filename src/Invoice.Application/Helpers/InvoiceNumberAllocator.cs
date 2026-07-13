namespace Invoice.Application.Helpers;

/// <summary>
/// Allocates the next per-user invoice number and commits, retrying when a parallel
/// create wins the race on the unique (UserId, InvoiceNumber) index.
/// </summary>
public static class InvoiceNumberAllocator
{
    private const int MaxAttempts = 3;

    public static async Task CommitWithAllocatedNumberAsync(IUnitOfWork uow, Domain.Entities.Invoice invoice)
    {
        for (var attempt = 1; ; attempt++)
        {
            invoice.InvoiceNumber = await uow.InvoiceRepository.GetNextInvoiceNumberAsync(invoice.UserId);
            try
            {
                await uow.CommitAsync();
                return;
            }
            catch (UniqueConstraintViolationException ex)
            {
                if (ex.ConstraintName?.Contains("InvoiceNumber") != true || attempt >= MaxAttempts)
                {
                    throw;
                }
            }
        }
    }
}
