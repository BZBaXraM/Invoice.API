using System.Text;
using FluentValidation;
using InvoiceManager.API.Auth;
using InvoiceManager.API.Data;
using InvoiceManager.API.Data.Entity;
using InvoiceManager.API.DTOs.Validation;
using InvoiceManager.API.Providers;
using InvoiceManager.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace InvoiceManager.API;

/// <summary>
/// Di - Dependency Injection class.
/// </summary>
public static class Di
{
    /// <summary>
    /// AddRepositories method.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Invoice.API",
                Version = "v1"
            });
            var path = Path.Combine(AppContext.BaseDirectory, "InvoiceManager.API.xml");
            setup.IncludeXmlComments(path);

            setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\""
            });

            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddDbContext<InvoiceDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("IdentityConnection"));
        });

        services.AddScoped<IAsyncUserService, UserService>();
        services.AddScoped<IAsyncCustomerService, CustomerService>();
        services.AddScoped<IAsyncInvoiceService, InvoiceService>();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }

    /// <summary>
    /// AuthenticationAndAuthorization method.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AuthenticationAndAuthorization(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRequestUserProvider, RequestUserProvider>();
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IJwtService, JwtService>();

        JwtConfig jwtConfig = new();
        configuration.GetSection("JWT").Bind(jwtConfig);
        services.AddSingleton(jwtConfig);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanTest", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.Requirements.Add(new CanTestRequirement());
            });
        });

        return services;
    }
}