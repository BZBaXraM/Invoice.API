namespace Invoice.Infrastructure.Data;

public class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Domain.Entities.Invoice> Invoices => Set<Domain.Entities.Invoice>();
    public DbSet<InvoiceRow> InvoiceRows => Set<InvoiceRow>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RecurringInvoice> RecurringInvoices => Set<RecurringInvoice>();
    public DbSet<RecurringInvoiceRow> RecurringInvoiceRows => Set<RecurringInvoiceRow>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
            v => v.ToUniversalTime(),
            v => v);
        var nullableDateTimeOffsetConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(dateTimeOffsetConverter);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(nullableDateTimeOffsetConverter);
                }
            }
        }

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasOne<User>()
            .WithMany(u => u.Customers)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Domain.Entities.Invoice>()
            .HasOne<User>()
            .WithMany(u => u.Invoices)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Domain.Entities.Invoice>()
            .HasOne<Customer>()
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Domain.Entities.Invoice>()
            .HasMany(i => i.Rows)
            .WithOne()
            .HasForeignKey(r => r.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Domain.Entities.Invoice>()
            .HasIndex(i => new { i.UserId, i.InvoiceNumber })
            .IsUnique();

        modelBuilder.Entity<Domain.Entities.Invoice>(invoice =>
        {
            invoice.Property(i => i.VatRate).HasPrecision(5, 2);
            invoice.Property(i => i.DiscountValue).HasPrecision(18, 2);
            invoice.Property(i => i.Subtotal).HasPrecision(18, 2);
            invoice.Property(i => i.DiscountAmount).HasPrecision(18, 2);
            invoice.Property(i => i.VatAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CompanyProfile>()
            .HasOne<User>()
            .WithOne(u => u.CompanyProfile)
            .HasForeignKey<CompanyProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CompanyProfile>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        modelBuilder.Entity<Domain.Entities.Invoice>()
            .HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>(payment =>
        {
            payment.Property(p => p.Amount).HasPrecision(18, 2);
            payment.HasIndex(p => p.UserId);
        });

        modelBuilder.Entity<RecurringInvoice>(recurring =>
        {
            recurring.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            recurring.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            recurring.HasMany(r => r.Rows)
                .WithOne()
                .HasForeignKey(row => row.RecurringInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            recurring.Property(r => r.VatRate).HasPrecision(5, 2);
            recurring.Property(r => r.DiscountValue).HasPrecision(18, 2);
            recurring.HasIndex(r => new { r.IsActive, r.NextRunDate });
        });

        modelBuilder.Entity<RecurringInvoiceRow>(row =>
        {
            row.Property(r => r.Quantity).HasPrecision(18, 2);
            row.Property(r => r.Rate).HasPrecision(18, 2);
        });

        // No FK to audited entities: log rows must survive hard deletes.
        modelBuilder.Entity<AuditLog>(audit =>
        {
            audit.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            audit.HasIndex(a => new { a.UserId, a.CreatedAt });
            audit.HasIndex(a => new { a.UserId, a.EntityType, a.EntityId });
        });
    }
}
