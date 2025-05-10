using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Settings;
using System.Reflection;
using System.Text;

namespace SurveyBasket.Api;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        var ConnectionString = configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Connection String 'DefaultConnection' Not Found");


        services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(ConnectionString));

        services.AddControllers();
        services.AddHybridCache();

        services.Configure<MailSetting>(configuration.GetSection(nameof(MailSetting)));

        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
        services.AddCors(options =>
            options.AddPolicy("MyPolicy", builder =>
            {
                builder.WithOrigins(allowedOrigins!)
                .AllowAnyMethod()
                .AllowAnyHeader();
            })
        );

        services
            .AddAuthConfig(configuration)
            .AddSwaggerService()
            .AddMapsterConfig()
            .AddFluentValidationConfig()
            .AddBackgroundJobsConfig(configuration);

        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INotificationService, NotificationService>();
        

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
    public static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation()
          .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    public static IServiceCollection AddSwaggerService(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
    public static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {

        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        return services;
    }
    public static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddOptions<JwtOptions>()
             .BindConfiguration(nameof(JwtOptions))
             .ValidateDataAnnotations()
             .ValidateOnStart();

        var jwtSettings = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();


        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.SaveToken = true;
            o.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.issuer,
                ValidAudience = jwtSettings?.audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecurityKey))

            };

        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        });

        return services;
    }

    public static IServiceCollection AddBackgroundJobsConfig(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        services.AddHangfireServer();
        return services;
    }

}
