namespace Invoice.Application.ServiceContracts;

public interface ICompanyProfileService
{
    Task<ResponseModel<CompanyProfileResponse?>> GetAsync(Guid ownerUserId);
    Task<ResponseModel<CompanyProfileResponse>> UpsertAsync(Guid ownerUserId, UpsertCompanyProfileRequest request);
    Task<ResponseModel> UploadImageAsync(Guid ownerUserId, CompanyImageKind kind, byte[] content, string contentType);
    Task<ResponseModel> DeleteImageAsync(Guid ownerUserId, CompanyImageKind kind);
    Task<ResponseModel<(byte[] Content, string ContentType)>> GetImageAsync(Guid ownerUserId, CompanyImageKind kind);
}
