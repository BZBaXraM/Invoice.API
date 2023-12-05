using Invoice.API.Models;
using Invoice.API.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Invoice.API.Data;

public class InvoiceContext(DbContextOptions<InvoiceContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Models.Invoice> Invoices => Set<Models.Invoice>();
    public DbSet<User> Users => Set<User>();

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     var invoices = new List<Models.Invoice>
    //     {
    //         new()
    //         {
    //             Id = Guid.NewGuid(),
    //             StartDate = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
    //             CustomerId = Guid.NewGuid(),
    //             Status = InvoiceStatus.Paid,
    //             Rows = new List<InvoiceRow>
    //             {
    //                 new()
    //                 {
    //                     Id = Guid.NewGuid(),
    //                     InvoiceId = Guid.NewGuid(),
    //                     Service = "Work 1",
    //                     Quantity = 1,
    //                     Rate = 1000
    //                 },
    //                 new()
    //                 {
    //                     Id = Guid.NewGuid(),
    //                     InvoiceId = Guid.NewGuid(),
    //                     Service = "Work 2",
    //                     Quantity = 2,
    //                     Rate = 2000
    //                 }
    //             }
    //         }
    //     };
    //
    //     var customers = new List<Customer>()
    //     {
    //         new()
    //         {
    //             Id = Guid.NewGuid(),
    //             Name = "Customer 1",
    //             Address = "Address 1",
    //             Email = "tea@tr.com",
    //             Phone = "123456789",
    //             Password = "123456789",
    //             CreatedAt = DateTimeOffset.UtcNow,
    //             UpdatedAt = DateTimeOffset.UtcNow
    //         }
    //     };
    //
    //     modelBuilder.Entity<Models.Invoice>().HasData(invoices);
    //     modelBuilder.Entity<Customer>().HasData(customers);
    // }
}