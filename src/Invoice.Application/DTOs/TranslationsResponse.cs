namespace Invoice.Application.DTOs;

public class TranslationsResponse
{
    public string Language { get; set; } = string.Empty;
    public Dictionary<string, string> Translations { get; set; } = new();
}
