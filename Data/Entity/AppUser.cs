using InvoiceManager.API.Models;
using Microsoft.AspNetCore.Identity;

namespace InvoiceManager.API.Data.Entity;

public class AppUser : IdentityUser
{
    public string? RefreshToken { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}