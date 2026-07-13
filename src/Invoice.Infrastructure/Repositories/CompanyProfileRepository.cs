namespace Invoice.Infrastructure.Repositories;

public class CompanyProfileRepository(InvoiceDbContext context) : ICompanyProfileRepository
{
    public async Task<CompanyProfile?> GetByUserIdAsync(Guid userId) =>
        await context.CompanyProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

    public CompanyProfile AddCompanyProfile(CompanyProfile profile)
    {
        context.CompanyProfiles.Add(profile);
        return profile;
    }
}
