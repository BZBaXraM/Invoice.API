using Invoice.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Invoice.API.Data;

public class InvoiceContext(DbContextOptions<InvoiceContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Models.Invoice> Invoices => Set<Models.Invoice>();
    public DbSet<User> Users => Set<User>();
}