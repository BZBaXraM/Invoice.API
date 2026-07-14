using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Invoice.Infrastructure.Services;

/// <summary>
/// Runs pg_dump (custom format) against the configured database and returns the dump bytes.
/// Requires the pg_dump binary in PATH — the API Docker image installs postgresql-client-17.
/// Restore with: pg_restore --clean --if-exists -d InvoiceDb backup-*.dump
/// </summary>
public class PgDumpBackupService(IConfiguration configuration, ILogger<PgDumpBackupService> logger) : IDatabaseBackupService
{
    public async Task<ResponseModel<DatabaseBackupResult>> CreateDumpAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                               configuration.GetConnectionString("DockerConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return ResponseModel.Failure<DatabaseBackupResult>("admin.backup.error", 500);
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        var startInfo = new ProcessStartInfo
        {
            FileName = "pg_dump",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("--format=custom");
        startInfo.ArgumentList.Add($"--host={builder.Host}");
        startInfo.ArgumentList.Add($"--port={builder.Port}");
        startInfo.ArgumentList.Add($"--username={builder.Username}");
        startInfo.ArgumentList.Add($"--dbname={builder.Database}");
        startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password ?? string.Empty;

        try
        {
            using var process = Process.Start(startInfo)!;

            using var output = new MemoryStream();
            var copyTask = process.StandardOutput.BaseStream.CopyToAsync(output, cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            await copyTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                logger.LogError("pg_dump exited with code {ExitCode}: {Error}", process.ExitCode, stderr);
                return ResponseModel.Failure<DatabaseBackupResult>("admin.backup.error", 500);
            }

            return ResponseModel.Success(new DatabaseBackupResult
            {
                Content = output.ToArray(),
                FileName = $"backup-{DateTimeOffset.UtcNow:yyyyMMdd-HHmm}.dump"
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to run pg_dump");
            return ResponseModel.Failure<DatabaseBackupResult>("admin.backup.error", 500);
        }
    }
}
