namespace Invoice.API.Controllers;

/// <summary>
/// Serves UI translation dictionaries. Anonymous access is intentional: login/register/
/// confirm-email/forgot-password/reset-password pages need translated text before a JWT exists.
/// </summary>
[AllowAnonymous]
[Route("api/translations")]
[ApiController]
public class TranslationController(ITranslationService service) : ControllerBase
{
    /// <summary>
    /// Gets the translation dictionary for the given language code, falling back to the default language.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseModel<TranslationsResponse>>> GetTranslations([FromQuery] string? lang)
    {
        var result = await service.GetTranslationsAsync(lang);
        return StatusCode(result.StatusCode, result);
    }
}
