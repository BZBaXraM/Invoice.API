using Microsoft.AspNetCore.Identity;

namespace Invoice.API.Models;

public class AppUser : IdentityUser
{
    public string? RefreshToken { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<InvoiceRow> InvoiceRows { get; set; } = new List<InvoiceRow>();
}