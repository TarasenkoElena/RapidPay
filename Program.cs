using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RapidPay.Auth;
using RapidPay.Cards;
using RapidPay.Payments;
using RapidPay.Payments.Fee;
using RapidPay.Utils;
using System.Text;
using RapidPay;
using StackExchange.Redis;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));

// Add services to the container.
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    builder.Services.AddDbContext<RapidPayDbContext>(options
        => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<RapidPayDbContext>(options
        => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}
builder.Services.AddMemoryCache();

AddServices(builder.Services, builder.Configuration);
AddAuthentification(builder);
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<RapidPayDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("RedisConnection")!, "redis");

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RapidPay API", Version = "v1" });

    c.EnableAnnotations();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        In = ParameterLocation.Header,
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId("swagger-ui");
        c.OAuthClientSecret("swagger-ui-secret");
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
}

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddAuthentification(WebApplicationBuilder builder)
{
    builder.Services.AddIdentity<ApiUser, IdentityRole>()
        .AddEntityFrameworkStores<RapidPayDbContext>()
        .AddDefaultTokenProviders();

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                ValidIssuer = jwtSettings["ValidIssuer"],
                ValidAudience = jwtSettings["ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
            };
        });
}

static void AddServices(IServiceCollection services, ConfigurationManager configuration)
{
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ICardService, CardService>();
    services.AddScoped<IPaymentService, PaymentService>();

    services.AddSingleton<ISaltGenerator, SaltGenerator>();
    services.AddSingleton<IHasher, Sha256Hasher>();
    services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
    services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

    services.AddSingleton<IFeeProvider, FeeProvider>();
    services.AddSingleton<IExternalBankCardBalanceProvider, FakeExternalBankCardBalanceProvider>();

    services.AddSingleton<IUniversalFeesExchangeService, UniversalFeesExchangeService>();
    services.Decorate<IUniversalFeesExchangeService, CachedUniversalFeesExchangeService>();
    services.AddSingleton<IFeeStorage, RedisFeeStorage>();

    services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")!));

    services.AddTransient<IValidator<Card>, CardValidator>();
    services.AddTransient<IValidator<Payment>, PaymentValidator>();
}