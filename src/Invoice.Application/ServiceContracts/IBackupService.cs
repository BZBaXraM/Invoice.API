namespace Invoice.Application.ServiceContracts;

public interface IBackupService
{
    /// <summary>Full-database backup, admin only.</summary>
    Task<ResponseModel<FullBackupResponse>> ExportAllAsync();

    /// <summary>Everything belonging to one user, without credential material.</summary>
    Task<ResponseModel<UserDataExportResponse>> ExportUserDataAsync(Guid userId);
}
