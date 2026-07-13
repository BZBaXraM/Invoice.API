namespace Invoice.Application.ServiceContracts;

/// <summary>
/// The kind of image stored on a company profile.
/// </summary>
public enum CompanyImageKind
{
    Logo,
    Signature
}

public interface ICompanyProfileService
{
    Task<ResponseModel<CompanyProfileResponse?>> GetAsync(Guid ownerUserId);
    Task<ResponseModel<CompanyProfileResponse>> UpsertAsync(Guid ownerUserId, UpsertCompanyProfileRequest request);
    Task<ResponseModel> UploadImageAsync(Guid ownerUserId, CompanyImageKind kind, byte[] content, string contentType);
    Task<ResponseModel> DeleteImageAsync(Guid ownerUserId, CompanyImageKind kind);
    Task<ResponseModel<(byte[] Content, string ContentType)>> GetImageAsync(Guid ownerUserId, CompanyImageKind kind);
}
