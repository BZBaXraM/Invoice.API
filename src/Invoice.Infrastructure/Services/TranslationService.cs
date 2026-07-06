namespace Invoice.Infrastructure.Services;

public class TranslationService : ITranslationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translationsByLanguage;

    public TranslationService(IHostEnvironment env)
    {
        _translationsByLanguage = LocalizationConstants.SupportedLanguages
            .ToDictionary(lang => lang, lang => LoadLanguageFile(env, lang));
    }

    public Task<ResponseModel<TranslationsResponse>> GetTranslationsAsync(string? languageCode)
    {
        var normalized = NormalizeLanguage(languageCode);

        var response = new TranslationsResponse
        {
            Language = normalized,
            Translations = _translationsByLanguage[normalized]
        };

        return Task.FromResult(ResponseModel.Success(response));
    }

    private string NormalizeLanguage(string? languageCode)
    {
        var code = languageCode?.Trim().ToLowerInvariant().Split('-')[0];

        return !string.IsNullOrEmpty(code) && _translationsByLanguage.ContainsKey(code)
            ? code
            : LocalizationConstants.DefaultLanguage;
    }

    private static Dictionary<string, string> LoadLanguageFile(IHostEnvironment env, string lang)
    {
        var path = Path.Combine(env.ContentRootPath, "Resources", "Localization", $"{lang}.json");
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }
}
