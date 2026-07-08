namespace Invoice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InvoiceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection") ??
                              configuration.GetConnectionString("DockerConnection")));

        JwtConfig jwtConfig = new();
        configuration.GetSection("JWT").Bind(jwtConfig);
        services.AddSingleton(jwtConfig);

        EmailConfig emailConfig = new();
        configuration.GetSection("EmailConfig").Bind(emailConfig);
        services.AddSingleton(emailConfig);
        services.AddSingleton<SmtpClient>();

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
                    }
                };
            });

        services.AddHttpContextAccessor();
        services.AddSignalR();

        services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<ICurrentUserService, CurrentUserService>()
            .AddSingleton<IEmailService, EmailService>()
            .AddSingleton<IBlackListService, BlackListService>()
            .AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>()
            .AddSingleton<ITranslationService, TranslationService>();

        return services;
    }
}