namespace Invoice.Application.Services;

public class CompanyProfileService(
    IUnitOfWork uow,
    UpsertCompanyProfileRequestValidator upsertValidator) : ICompanyProfileService
{
    private const int MaxImageBytes = 2 * 1024 * 1024;
    private static readonly string[] AllowedImageContentTypes = ["image/png", "image/jpeg"];

    public async Task<ResponseModel<CompanyProfileResponse?>> GetAsync(Guid ownerUserId)
    {
        var profile = await uow.CompanyProfileRepository.GetByUserIdAsync(ownerUserId);
        return ResponseModel.Success(profile?.ToCompanyProfileResponse());
    }

    public async Task<ResponseModel<CompanyProfileResponse>> UpsertAsync(Guid ownerUserId, UpsertCompanyProfileRequest request)
    {
        var validation = await upsertValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<CompanyProfileResponse>("validation.failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var profile = await uow.CompanyProfileRepository.GetByUserIdAsync(ownerUserId);
        if (profile is null)
        {
            profile = new CompanyProfile
            {
                UserId = ownerUserId,
                CompanyName = request.CompanyName,
                Voen = request.Voen,
                CreatedAt = DateTimeOffset.UtcNow
            };
            uow.CompanyProfileRepository.AddCompanyProfile(profile);
        }

        profile.CompanyName = request.CompanyName;
        profile.Voen = request.Voen;
        profile.BankName = request.BankName;
        profile.BankVoen = request.BankVoen;
        profile.Iban = request.Iban;
        profile.BankAccount = request.BankAccount;
        profile.SwiftCode = request.SwiftCode;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await uow.CommitAsync();

        return ResponseModel.Success(profile.ToCompanyProfileResponse());
    }

    public async Task<ResponseModel> UploadImageAsync(Guid ownerUserId, CompanyImageKind kind, byte[] content, string contentType)
    {
        if (content.Length == 0)
        {
            return ResponseModel.Failure("companyProfile.error.imageEmpty", 400);
        }

        if (content.Length > MaxImageBytes)
        {
            return ResponseModel.Failure("companyProfile.error.imageTooLarge", 400);
        }

        if (!AllowedImageContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return ResponseModel.Failure("companyProfile.error.imageUnsupportedType", 400);
        }

        var profile = await uow.CompanyProfileRepository.GetByUserIdAsync(ownerUserId);
        if (profile is null)
        {
            return ResponseModel.Failure("companyProfile.error.notFound", 404);
        }

        if (kind == CompanyImageKind.Logo)
        {
            profile.LogoImage = content;
            profile.LogoContentType = contentType;
        }
        else
        {
            profile.SignatureImage = content;
            profile.SignatureContentType = contentType;
        }

        profile.UpdatedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        return ResponseModel.Success("companyProfile.imageUploaded");
    }

    public async Task<ResponseModel> DeleteImageAsync(Guid ownerUserId, CompanyImageKind kind)
    {
        var profile = await uow.CompanyProfileRepository.GetByUserIdAsync(ownerUserId);
        if (profile is null)
        {
            return ResponseModel.Failure("companyProfile.error.notFound", 404);
        }

        if (kind == CompanyImageKind.Logo)
        {
            profile.LogoImage = null;
            profile.LogoContentType = null;
        }
        else
        {
            profile.SignatureImage = null;
            profile.SignatureContentType = null;
        }

        profile.UpdatedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        return ResponseModel.Success("companyProfile.imageDeleted");
    }

    public async Task<ResponseModel<(byte[] Content, string ContentType)>> GetImageAsync(Guid ownerUserId, CompanyImageKind kind)
    {
        var profile = await uow.CompanyProfileRepository.GetByUserIdAsync(ownerUserId);
        var (content, contentType) = kind == CompanyImageKind.Logo
            ? (profile?.LogoImage, profile?.LogoContentType)
            : (profile?.SignatureImage, profile?.SignatureContentType);

        if (content is not { Length: > 0 } || contentType is null)
        {
            return ResponseModel.Failure<(byte[], string)>("companyProfile.error.imageNotFound", 404);
        }

        return ResponseModel.Success((content, contentType));
    }
}
