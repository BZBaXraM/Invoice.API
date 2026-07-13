namespace Invoice.API.Controllers;

/// <summary>
/// Controller for the user's company profile (seller requisites, logo, signature).
/// </summary>
[Authorize(Policy = AuthPolicies.NotAdmin)]
[Route("api/company-profile")]
[ApiController]
public class CompanyProfileController(ICompanyProfileService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Gets the current user's company profile. Returns null data if none exists yet.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseModel<CompanyProfileResponse?>>> GetProfile()
    {
        var result = await service.GetAsync(currentUserService.UserId!.Value);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Creates or updates the current user's company profile.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ResponseModel<CompanyProfileResponse>>> UpsertProfile(
        [FromBody] UpsertCompanyProfileRequest request)
    {
        var result = await service.UpsertAsync(currentUserService.UserId!.Value, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Uploads the company logo (png/jpeg, max 2 MB).
    /// </summary>
    [HttpPost("logo")]
    public Task<ActionResult<ResponseModel>> UploadLogo(IFormFile file) =>
        UploadImageAsync(CompanyImageKind.Logo, file);

    /// <summary>
    /// Uploads the signature image (png/jpeg, max 2 MB).
    /// </summary>
    [HttpPost("signature")]
    public Task<ActionResult<ResponseModel>> UploadSignature(IFormFile file) =>
        UploadImageAsync(CompanyImageKind.Signature, file);

    /// <summary>
    /// Returns the company logo image bytes.
    /// </summary>
    [HttpGet("logo")]
    public Task<IActionResult> GetLogo() => GetImageAsync(CompanyImageKind.Logo);

    /// <summary>
    /// Returns the signature image bytes.
    /// </summary>
    [HttpGet("signature")]
    public Task<IActionResult> GetSignature() => GetImageAsync(CompanyImageKind.Signature);

    /// <summary>
    /// Removes the company logo.
    /// </summary>
    [HttpDelete("logo")]
    public async Task<ActionResult<ResponseModel>> DeleteLogo()
    {
        var result = await service.DeleteImageAsync(currentUserService.UserId!.Value, CompanyImageKind.Logo);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Removes the signature image.
    /// </summary>
    [HttpDelete("signature")]
    public async Task<ActionResult<ResponseModel>> DeleteSignature()
    {
        var result = await service.DeleteImageAsync(currentUserService.UserId!.Value, CompanyImageKind.Signature);
        return StatusCode(result.StatusCode, result);
    }

    private async Task<ActionResult<ResponseModel>> UploadImageAsync(CompanyImageKind kind, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            var failure = ResponseModel.Failure("companyProfile.error.imageEmpty", 400);
            return StatusCode(failure.StatusCode, failure);
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        var result = await service.UploadImageAsync(
            currentUserService.UserId!.Value, kind, stream.ToArray(), file.ContentType);
        return StatusCode(result.StatusCode, result);
    }

    private async Task<IActionResult> GetImageAsync(CompanyImageKind kind)
    {
        var result = await service.GetImageAsync(currentUserService.UserId!.Value, kind);
        if (result.IsFailed)
        {
            return StatusCode(result.StatusCode, ResponseModel.Failure(result.Message, result.StatusCode));
        }

        return File(result.Data.Content, result.Data.ContentType);
    }
}
