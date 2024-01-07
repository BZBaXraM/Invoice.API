using Invoice.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Invoice.API.Data;

public class AuthContext(DbContextOptions<AuthContext> options) : IdentityDbContext<AppUser>(options)
{
    public override DbSet<AppUser> Users => Set<AppUser>();
}