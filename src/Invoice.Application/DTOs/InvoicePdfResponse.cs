namespace Invoice.Application.DTOs;

/// <summary>
/// A rendered invoice PDF plus its suggested download file name.
/// </summary>
public class InvoicePdfResponse
{
    /// <summary>The PDF bytes.</summary>
    public required byte[] Content { get; set; }

    /// <summary>The suggested file name, e.g. invoice-INV-0007.pdf.</summary>
    public required string FileName { get; set; }
}
