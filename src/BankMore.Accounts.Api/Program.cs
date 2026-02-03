using BankMore.Accounts.Api.Infrastructure.Security.Users;
using BankMore.Accounts.Api.Infrastructure.Persistence;

//JWT
using BankMore.Accounts.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ##USER_AS_SECURITY_PATHING
builder.Services.AddScoped<IUsuarioAuthClient, UsuarioAuthStub>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(
    new BankMore.Accounts.Api.Infrastructure.Persistence.SqliteConnectionFactory(
        "Data Source=bankmore_accounts.db"));

builder.Services.AddSingleton<SqliteSchemaInitializer>();

builder.Services.AddScoped<BankMore.Accounts.Api.Domain.Repositories.IContaCorrenteRepository,
    BankMore.Accounts.Api.Infrastructure.Repositories.ContaCorrenteRepository>();

// builder.Services.Configure<BankMore.Accounts.Api.Infrastructure.Security.JwtOptions>(
//     builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<
    BankMore.Accounts.Api.Domain.Repositories.IMovimentoRepository,
    BankMore.Accounts.Api.Infrastructure.Repositories.MovimentoRepository>();

builder.Services.AddScoped<
    BankMore.Accounts.Api.Application.Services.IMovimentacaoService,
    BankMore.Accounts.Api.Application.Services.MovimentacaoService>();

builder.Services.AddScoped<
    BankMore.Accounts.Api.Application.Services.ISaldoService,
    BankMore.Accounts.Api.Application.Services.SaldoService>();

//JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:SecretKey"]!);

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



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<SqliteSchemaInitializer>();

    //try {
    initializer.Initialize();
    //} catch (Exception ex) { throw; }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<BankMore.Accounts.Api.Infrastructure.Security.ExceptionMiddleware>();

//JWT
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
