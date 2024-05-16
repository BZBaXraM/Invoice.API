using InvoiceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.API.Data;

/// <inheritdoc />
public class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the users table.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets or sets the customers table.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets or sets the invoices table.
    /// </summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <summary>
    /// Gets or sets the invoice rows table.
    /// </summary>
    public DbSet<InvoiceRow> InvoiceRows => Set<InvoiceRow>();

    /// <summary>
    /// Configure the relationships between the entities.
    /// </summary>
    /// <param name="modelBuilder"></param>
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

        modelBuilder.Entity<Customer>()
            .HasKey(c => c.Id);
    }
}