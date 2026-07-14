namespace Invoice.Application.ServiceContracts;

/// <summary>
/// Produces a real, restorable database backup. Implemented in Infrastructure
/// over pg_dump — Application stays free of provider/process concerns.
/// </summary>
public interface IDatabaseBackupService
{
    Task<ResponseModel<DatabaseBackupResult>> CreateDumpAsync(CancellationToken cancellationToken = default);
}
