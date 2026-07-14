namespace Invoice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<InvoiceDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection") ??
                              configuration.GetConnectionString("DockerConnection"))
                .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        JwtConfig jwtConfig = new();
        configuration.GetSection("JWT").Bind(jwtConfig);
        services.AddSingleton(jwtConfig);

        EmailConfig emailConfig = new();
        configuration.GetSection("EmailConfig").Bind(emailConfig);
        services.AddSingleton(emailConfig);

        GroqConfig groqConfig = new();
        configuration.GetSection("Groq").Bind(groqConfig);
        services.AddSingleton(groqConfig);
        services.AddHttpClient<IAiChatClient, GroQChatClient>(client =>
        {
            client.BaseAddress = new Uri(groqConfig.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", groqConfig.ApiKey);
        });

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

                // Browsers can't set the Authorization header on a WebSocket handshake, so
                // the SignalR client sends the JWT as an "access_token" query parameter instead.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    // A disabled (or hard-deleted) user must lose access immediately, not when
                    // their access token expires — the in-memory blacklist can't cover this
                    // because the admin never sees the victim's token string.
                    OnTokenValidated = async context =>
                    {
                        var userIdRaw = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!Guid.TryParse(userIdRaw, out var userId))
                        {
                            context.Fail("Invalid token");
                            return;
                        }

                        var uow = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
                        var user = await uow.UserRepository.GetByIdAsync(userId);
                        if (user is null || !user.IsActive)
                        {
                            context.Fail("Account is disabled");
                        }
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            // Deny-only check (rather than RequireRole("User")) so access tokens
            // issued before the role claim existed keep working until they expire.
            options.AddPolicy(AuthPolicies.NotAdmin, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(context => !context.User.IsInRole(nameof(UserRole.Admin))));
        });

        services.AddHttpContextAccessor();
        services.AddSignalR();

        services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<ICurrentUserService, CurrentUserService>()
            .AddScoped<IDatabaseBackupService, PgDumpBackupService>()
            .AddSingleton<IEmailService, EmailService>()
            .AddSingleton<IBlackListService, BlackListService>()
            .AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>()
            .AddSingleton<ITranslationService, TranslationService>();

        services.AddHostedService<InvoiceSchedulerHostedService>();

        return services;
    }
}