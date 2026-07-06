namespace Invoice.Application.ServiceContracts;

public interface ITranslationService
{
    Task<ResponseModel<TranslationsResponse>> GetTranslationsAsync(string? languageCode);
}
