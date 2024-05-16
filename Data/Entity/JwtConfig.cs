namespace InvoiceManager.API.Data.Entity;

public class JwtConfig
{
    /// <summary>
    /// Secret
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Expiration
    /// </summary>
    public int Expiration { get; set; }
}