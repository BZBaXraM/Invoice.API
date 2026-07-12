namespace Invoice.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddScoped<RegisterRequestValidator>()
            .AddScoped<LoginRequestValidator>()
            .AddScoped<ConfirmEmailCodeRequestValidator>()
            .AddScoped<ResetPasswordRequestValidator>()
            .AddScoped<UpdateProfileRequestValidator>()
            .AddScoped<ChangePasswordRequestValidator>()
            .AddScoped<CreateCustomerRequestValidator>()
            .AddScoped<UpdateCustomerRequestValidator>()
            .AddScoped<CreateInvoiceRequestValidator>()
            .AddScoped<UpdateInvoiceRequestValidator>()
            .AddValidatorsFromAssemblyContaining<RegisterRequestValidator>()
            .AddScoped<IAccountService, AccountService>()
            .AddScoped<IAdminService, AdminService>()
            .AddScoped<ICustomerService, CustomerService>()
            .AddScoped<IInvoiceService, InvoiceService>()
            .AddScoped<IReportService, ReportService>()
            .AddScoped<IChatService, ChatService>();

        return services;
    }
}
