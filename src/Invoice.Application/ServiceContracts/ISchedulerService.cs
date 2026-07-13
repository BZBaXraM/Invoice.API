namespace Invoice.Application.ServiceContracts;

/// <summary>
/// Periodic maintenance run by the background scheduler: generates invoices from due
/// recurring templates and flags unpaid past-due invoices as Overdue.
/// </summary>
public interface ISchedulerService
{
    Task ProcessAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
}
