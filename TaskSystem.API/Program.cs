using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskSystem.API.Middleware;
using TaskSystem.Application.Commands.Admin;
using TaskSystem.Application.Commands.Auth.AuthLogin;
using TaskSystem.Application.Commands.Auth.AuthRefreshToken;
using TaskSystem.Application.Commands.Auth.AuthRegister;
using TaskSystem.Application.Commands.Users.UserChangePassword;
using TaskSystem.Application.Commands.Users.UserDelete;
using TaskSystem.Application.Commands.Users.UserRegister;
using TaskSystem.Application.Commands.Users.UserUpdate;
using TaskSystem.Application.Commands.Uzduotys.ResetLast;
using TaskSystem.Application.Commands.Uzduotys.UzduotisCreate;
using TaskSystem.Application.Commands.Uzduotys.UzduotisDelete;
using TaskSystem.Application.Commands.Uzduotys.UzduotisUpdate;
using TaskSystem.Application.Common;
using TaskSystem.Application.Mapping;
using TaskSystem.Application.Queries.Users;
using TaskSystem.Application.Queries.Users.GetUserById;
using TaskSystem.Application.Queries.Uzduotys.GetAllUzduotys;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserProfileId;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotisById;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserEmail;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserProfileId;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Admin;
using TaskSystem.Infrastructure.Auth;
using TaskSystem.Infrastructure.Persistence;
using TaskSystem.Infrastructure.Repositories;
using TaskSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverter<string>());
        options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverter<int>());
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();

// Swagger su JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Įvesk tokeną: Bearer {token}",
        }
    );

    c.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// ------------------------------------------------------------
// JWT konfigūracija (saugiai iš User Secrets / Env Variables)
// ------------------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

// Guard rails: jei JWT Key nerastas → stabdom programą
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception(
        "JWT Key nerastas. Įsitikinkite, kad jis nustatytas User Secrets arba Environment Variables."
    );

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new Exception(
        "Connection string nerastas. Nustatykite jį User Secrets arba Environment Variables."
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder
    .Services.AddHealthChecks()
    .AddMySql(serviceProvider =>
        serviceProvider
            .GetRequiredService<IConfiguration>()
            .GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.")
    );
MapsterConfig.RegisterMappings();
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>();

builder.Services.AddScoped<AuthRegisterHandler>();
builder.Services.AddScoped<AuthLoginHandler>();
builder.Services.AddScoped<UserRegisterHandler>();
builder.Services.AddScoped<UserUpdateHandler>();
builder.Services.AddScoped<UserDeleteHandler>();
builder.Services.AddScoped<UserChangePasswordHandler>();
builder.Services.AddScoped<UserAdminUpdateHandler>();

builder.Services.AddScoped<UzduotisCreateHandler>();
builder.Services.AddScoped<UzduotisUpdateHandler>();
builder.Services.AddScoped<UzduotisResetLastHandler>();
builder.Services.AddScoped<UzduotisAdminDeleteHandler>();
builder.Services.AddScoped<UzduotisDeleteHandler>();

builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<GetUserByEmailHandler>();

builder.Services.AddScoped<GetUzduotisByIdHandler>();
builder.Services.AddScoped<GetUzduotysByUserProfileIdHandler>();
builder.Services.AddScoped<GetLastUzduotisByUserProfileIdHandler>();
builder.Services.AddScoped<GetAllUzduotysHandler>();
builder.Services.AddScoped<GetUzduotysByUserEmailHandler>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUzduotisRepository, UzduotisRepository>();
builder.Services.AddScoped<IAdminPromotionTokenRepository, AdminPromotionTokenRepository>();
builder.Services.AddScoped<AdminPromoteWithTokenHandler>();
builder.Services.AddScoped<AdminPromotionTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthRefreshTokenHandler>();
builder.Services.AddHostedService<TokenCleanupService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

var app = builder.Build();

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    return;
}

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

public partial class Program { }
