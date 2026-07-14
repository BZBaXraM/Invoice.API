namespace Invoice.Application.Services;

public class BackupService(IUnitOfWork uow) : IBackupService
{
    public async Task<ResponseModel<UserDataExportResponse>> ExportUserDataAsync(Guid userId)
    {
        var graph = await uow.BackupRepository.GetUserGraphAsync(userId);
        if (graph is null)
        {
            return ResponseModel.Failure<UserDataExportResponse>("User not found", 404);
        }

        return ResponseModel.Success(graph.ToUserDataExportResponse());
    }
}
