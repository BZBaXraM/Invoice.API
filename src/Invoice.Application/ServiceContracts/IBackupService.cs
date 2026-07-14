namespace Invoice.Application.ServiceContracts;

public interface IBackupService
{
    /// <summary>Everything belonging to one user, without credential material.</summary>
    Task<ResponseModel<UserDataExportResponse>> ExportUserDataAsync(Guid userId);
}
