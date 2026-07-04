using System.Text;
using Invoice.Application.RepositoryContracts;
using Invoice.Application.ServiceContracts;
using Invoice.Infrastructure.Configs;
using Invoice.Infrastructure.Data;
using Invoice.Infrastructure.Repositories;
using Invoice.Infrastructure.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Invoice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InvoiceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        JwtConfig jwtConfig = new();
        configuration.GetSection("JWT").Bind(jwtConfig);
        services.AddSingleton(jwtConfig);

        EmailConfig emailConfig = new();
        configuration.GetSection("EmailConfig").Bind(emailConfig);
        services.AddSingleton(emailConfig);
        services.AddSingleton<SmtpClient>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });

        services.AddHttpContextAccessor();

        services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<ICurrentUserService, CurrentUserService>()
            .AddSingleton<IEmailService, EmailService>()
            .AddSingleton<IBlackListService, BlackListService>();

        return services;
    }
}
