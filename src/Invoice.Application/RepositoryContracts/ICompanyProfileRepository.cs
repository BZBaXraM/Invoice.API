namespace Invoice.Application.RepositoryContracts;

public interface ICompanyProfileRepository
{
    Task<CompanyProfile?> GetByUserIdAsync(Guid userId);

    CompanyProfile AddCompanyProfile(CompanyProfile profile);
}
