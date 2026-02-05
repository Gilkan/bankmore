using BankMore.Accounts.Api.Application.Options;

using BankMore.Accounts.Api.Infrastructure.Security.Users;
using BankMore.Accounts.Api.Infrastructure.Persistence;
using BankMore.Accounts.Api.Infrastructure.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TarifaOptions>(
    builder.Configuration.GetSection("Tarifa"));

// USER AUTH
builder.Services.AddScoped<IUsuarioAuthClient, UsuarioAuthStub>();

// Controllers
builder.Services.AddControllers();

// Swagger + JWT config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// SQLite
builder.Services.AddSingleton(
    new SqliteConnectionFactory("Data Source=bankmore_accounts.db"));

builder.Services.AddSingleton<SqliteSchemaInitializer>();


// Repositories
builder.Services.AddScoped<
    BankMore.Accounts.Api.Domain.Repositories.IContaCorrenteRepository,
    BankMore.Accounts.Api.Infrastructure.Repositories.ContaCorrenteRepository>();

builder.Services.AddScoped<
    BankMore.Accounts.Api.Domain.Repositories.IMovimentoRepository,
    BankMore.Accounts.Api.Infrastructure.Repositories.MovimentoRepository>();

builder.Services.AddScoped<
    BankMore.Accounts.Api.Domain.Repositories.ITransferenciaRepository,
    BankMore.Accounts.Api.Infrastructure.Repositories.TransferenciaRepository>();

builder.Services.AddScoped<
    BankMore.Accounts.Api.Domain.Repositories.ITarifaRepository,
    BankMore.Accounts.Api.Infrastructure.Repositories.TarifaRepository>();

// Services
builder.Services.AddScoped<
    BankMore.Accounts.Api.Application.Services.ISaldoService,
    BankMore.Accounts.Api.Application.Services.SaldoService>();


// JWT
var secretKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(secretKey))
    throw new InvalidOperationException("Jwt:SecretKey não configurado.");

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddScoped<IUnitOfWork, SqliteUnitOfWork>();

builder.Services.AddMediatR(typeof(Program).Assembly);

var app = builder.Build();


// Inicialização schema
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<SqliteSchemaInitializer>();

    initializer.Initialize();
}


// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<
    BankMore.Accounts.Api.Infrastructure.Security.ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
