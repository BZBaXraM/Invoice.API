using InvoiceManager.API.Data.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.API.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public override DbSet<AppUser> Users => Set<AppUser>();
}