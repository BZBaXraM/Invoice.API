namespace Invoice.Application.Services;

public class BackupService(IUnitOfWork uow) : IBackupService
{
    public async Task<ResponseModel<FullBackupResponse>> ExportAllAsync()
    {
        var backup = new FullBackupResponse { ExportedAt = DateTimeOffset.UtcNow };

        foreach (var userId in await uow.BackupRepository.GetAllUserIdsAsync())
        {
            var graph = await uow.BackupRepository.GetUserGraphAsync(userId);
            if (graph is null)
            {
                continue;
            }

            backup.Users.Add(new BackupUserRecord
            {
                Id = graph.User.Id,
                FirstName = graph.User.FirstName,
                LastName = graph.User.LastName,
                Username = graph.User.Username,
                Email = graph.User.Email,
                PasswordHash = graph.User.Password,
                Address = graph.User.Address,
                PhoneNumber = graph.User.PhoneNumber,
                IsEmailConfirmed = graph.User.IsEmailConfirmed,
                Role = graph.User.Role.ToString(),
                IsActive = graph.User.IsActive,
                CreatedAt = graph.User.CreatedAt,
                UpdatedAt = graph.User.UpdatedAt,
                CompanyProfile = ToCompanyProfileExport(graph.CompanyProfile),
                Customers = graph.Customers.Select(c => c.ToCustomerResponse()).ToList(),
                Invoices = graph.Invoices.Select(i => i.ToInvoiceResponse()).ToList(),
                RecurringInvoices = graph.RecurringInvoices.Select(r => r.ToRecurringInvoiceResponse()).ToList(),
                AuditLogs = ToAuditLogResponses(graph.AuditLogs)
            });
        }

        return ResponseModel.Success(backup);
    }

    public async Task<ResponseModel<UserDataExportResponse>> ExportUserDataAsync(Guid userId)
    {
        var graph = await uow.BackupRepository.GetUserGraphAsync(userId);
        if (graph is null)
        {
            return ResponseModel.Failure<UserDataExportResponse>("User not found", 404);
        }

        return ResponseModel.Success(new UserDataExportResponse
        {
            ExportedAt = DateTimeOffset.UtcNow,
            Profile = graph.User.ToUserResponse(),
            CompanyProfile = ToCompanyProfileExport(graph.CompanyProfile),
            Customers = graph.Customers.Select(c => c.ToCustomerResponse()).ToList(),
            Invoices = graph.Invoices.Select(i => i.ToInvoiceResponse()).ToList(),
            RecurringInvoices = graph.RecurringInvoices.Select(r => r.ToRecurringInvoiceResponse()).ToList(),
            AuditLogs = ToAuditLogResponses(graph.AuditLogs)
        });
    }

    private static CompanyProfileExport? ToCompanyProfileExport(CompanyProfile? profile) => profile is null
        ? null
        : new CompanyProfileExport
        {
            Id = profile.Id,
            CompanyName = profile.CompanyName,
            Voen = profile.Voen,
            BankName = profile.BankName,
            BankVoen = profile.BankVoen,
            Iban = profile.Iban,
            BankAccount = profile.BankAccount,
            SwiftCode = profile.SwiftCode,
            LogoBase64 = profile.LogoImage is null ? null : Convert.ToBase64String(profile.LogoImage),
            LogoContentType = profile.LogoContentType,
            SignatureBase64 = profile.SignatureImage is null ? null : Convert.ToBase64String(profile.SignatureImage),
            SignatureContentType = profile.SignatureContentType,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };

    private static List<AuditLogResponse> ToAuditLogResponses(List<AuditLog> logs) => logs
        .Select(a => new AuditLogResponse
        {
            Id = a.Id,
            ActorName = a.ActorName,
            ActorEmail = a.ActorEmail,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Action = a.Action,
            ChangesJson = a.ChangesJson,
            CreatedAt = a.CreatedAt
        })
        .ToList();
}
