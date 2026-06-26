using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskSystem.API.Middleware;
using TaskSystem.Application.Commands.Users.ChangePassword;
using TaskSystem.Application.Commands.Users.DeleteUser;
using TaskSystem.Application.Commands.Users.LoginUser;
using TaskSystem.Application.Commands.Users.RegisterUser;
using TaskSystem.Application.Commands.Users.UpdateUser;
using TaskSystem.Application.Commands.Uzduotys.CreateUzduotis;
using TaskSystem.Application.Commands.Uzduotys.DeleteUzduotis;
using TaskSystem.Application.Commands.Uzduotys.ResetLastUzduotis;
using TaskSystem.Application.Commands.Uzduotys.UpdateUzduotis;
using TaskSystem.Application.Queries.Users.GetUserByEmail;
using TaskSystem.Application.Queries.Users.GetUserById;
using TaskSystem.Application.Queries.Uzduotys.GetAllUzduotys;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserId;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotisById;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserEmail;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserId;
using TaskSystem.Application.Validation.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Auth;
using TaskSystem.Infrastructure.Persistence;
using TaskSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

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

builder.Services.AddHealthChecks().AddMySql(connectionString);

builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<LoginUserHandler>();
builder.Services.AddScoped<AdminUpdateUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<ChangePasswordHandler>();

builder.Services.AddScoped<CreateUzduotisHandler>();
builder.Services.AddScoped<UpdateUzduotisHandler>();
builder.Services.AddScoped<ResetLastUzduotisHandler>();
builder.Services.AddScoped<AdminDeleteUzduotisHandler>();
builder.Services.AddScoped<DeleteUzduotisHandler>();

builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<GetUserByEmailHandler>();

builder.Services.AddScoped<GetUzduotisByIdHandler>();
builder.Services.AddScoped<GetUzduotysByUserIdHandler>();
builder.Services.AddScoped<GetLastUzduotisByUserIdHandler>();
builder.Services.AddScoped<GetAllUzduotysHandler>();
builder.Services.AddScoped<GetUzduotysByUserEmailHandler>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUzduotisRepository, UzduotisRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
