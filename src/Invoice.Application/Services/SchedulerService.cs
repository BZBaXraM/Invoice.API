namespace Invoice.Application.Services;

public class SchedulerService(
    IUnitOfWork uow,
    IRealtimeNotifier realtimeNotifier) : ISchedulerService
{
    /// <summary>
    /// Safety cap on catch-up generation per template per run (e.g. a weekly template
    /// that was inactive for months should not flood the account).
    /// </summary>
    private const int MaxGenerationsPerRun = 12;

    public async Task ProcessAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        await GenerateRecurringInvoicesAsync(now, cancellationToken);
        await MarkOverdueInvoicesAsync(now, cancellationToken);
    }

    private async Task GenerateRecurringInvoicesAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var dueTemplates = await uow.RecurringInvoiceRepository.GetDueAsync(now);

        foreach (var template in dueTemplates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var generated = new List<Domain.Entities.Invoice>();
            for (var i = 0; template.IsActive && template.NextRunDate <= now && i < MaxGenerationsPerRun; i++)
            {
                var periodStart = template.NextRunDate;
                var periodEnd = Advance(periodStart, template.Frequency).AddDays(-1);

                var invoice = new Domain.Entities.Invoice
                {
                    UserId = template.UserId,
                    CustomerId = template.CustomerId,
                    StartDate = periodStart,
                    EndDate = periodEnd,
                    DueDate = now.AddDays(template.DueInDays),
                    VatRate = template.VatRate,
                    DiscountType = template.DiscountType,
                    DiscountValue = template.DiscountValue,
                    Comment = template.Comment,
                    Status = InvoiceStatus.Created,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Rows = template.Rows.Select(r => new InvoiceRow
                    {
                        Service = r.Service,
                        Quantity = r.Quantity,
                        Rate = r.Rate,
                        Sum = InvoiceTotalsCalculator.RowSum(r.Quantity, r.Rate)
                    }).ToList()
                };

                InvoiceTotalsCalculator.ApplyTotals(invoice);
                uow.InvoiceRepository.AddInvoice(invoice);
                await InvoiceNumberAllocator.CommitWithAllocatedNumberAsync(uow, invoice);
                generated.Add(invoice);

                template.NextRunDate = Advance(template.NextRunDate, template.Frequency);
                if (template.EndDate.HasValue && template.NextRunDate > template.EndDate.Value)
                {
                    template.IsActive = false;
                }
            }

            template.UpdatedAt = DateTimeOffset.UtcNow;
            await uow.CommitAsync();

            foreach (var invoice in generated)
            {
                await realtimeNotifier.InvoiceCreatedAsync(template.UserId, invoice.ToInvoiceResponse());
            }
        }
    }

    private async Task MarkOverdueInvoicesAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var candidates = await uow.InvoiceRepository.GetOverdueCandidatesAsync(now);
        if (candidates.Count == 0)
        {
            return;
        }

        foreach (var invoice in candidates)
        {
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await uow.CommitAsync();

        foreach (var invoice in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await realtimeNotifier.InvoiceStatusChangedAsync(invoice.UserId, invoice.Id, InvoiceStatus.Overdue);
        }
    }

    private static DateTimeOffset Advance(DateTimeOffset date, RecurrenceFrequency frequency) => frequency switch
    {
        RecurrenceFrequency.Weekly => date.AddDays(7),
        RecurrenceFrequency.Monthly => date.AddMonths(1),
        RecurrenceFrequency.Quarterly => date.AddMonths(3),
        RecurrenceFrequency.Yearly => date.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null)
    };
}
