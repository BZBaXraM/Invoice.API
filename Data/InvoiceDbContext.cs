using InvoiceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.API.Data;

public class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceRow> InvoiceRows => Set<InvoiceRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Rows)
            .WithOne()
            .HasForeignKey(ir => ir.InvoiceId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Customers)
            .WithOne()
            .HasForeignKey(c => c.Id);
    }
}