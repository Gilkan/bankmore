using BankMore.Application.Options;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Messaging;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Persistence;
using BankMore.Infrastructure.Repositories;
using BankMore.Infrastructure.Security;
using BankMore.Infrastructure.Security.Users;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var tarifaSection = builder.Configuration.GetSection("Tarifa");
var tarifaOptions = new TarifaOptions(
    valorTransferencia: tarifaSection.GetValue<decimal>("ValorTransferencia")
);
builder.Services.AddSingleton<Microsoft.Extensions.Options.IOptions<TarifaOptions>>(
    Microsoft.Extensions.Options.Options.Create(tarifaOptions)
);

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

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


// =======================
// SQLite / Persistence
// =======================

var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString") ?? "Data Source=bankmore.db";

builder.Services.AddSingleton<IConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));


builder.Services.AddScoped<IUnitOfWork, SqliteUnitOfWork>();


// =======================
// Repositories
// =======================

builder.Services.AddScoped<IContaCorrenteRepository>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var numeroInicial = configuration.GetValue<int>("ContaCorrente:NumeroInicial");
    var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>();

    return new ContaCorrenteRepository(factory, numeroInicial, dbOptions);
});


builder.Services.AddScoped<IMovimentoRepository>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>();

    return new MovimentoRepository(factory, dbOptions);
});

builder.Services.AddScoped<ITransferenciaRepository>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>();

    return new TransferenciaRepository(factory, dbOptions);
});

builder.Services.AddScoped<ITarifaRepository>(sp =>
{
    var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>();

    return new TarifaRepository(dbOptions);
});



// =======================
// Services
// =======================

builder.Services.AddScoped<
    BankMore.Application.Services.ISaldoService,
    BankMore.Application.Services.SaldoService>();


// =======================
// JWT
// =======================

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


builder.Services.AddMediatR(typeof(Program).Assembly);


// =======================
// Kafka
// =======================

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<KafkaProducer>(sp =>
{
    var options = sp.GetRequiredService<IOptions<KafkaOptions>>();
    var logger = sp.GetRequiredService<ILogger<KafkaProducer>>();

    // If no bootstrap servers configured, disable Kafka
    if (string.IsNullOrWhiteSpace(options.Value.BootstrapServers))
        return null!;

    return new KafkaProducer(options, logger);
});


var app = builder.Build();


// =======================
// Pipeline
// =======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<
    BankMore.Infrastructure.Security.ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStopping.Register(() =>
{
    var producer = app.Services.GetService<KafkaProducer>();
    producer?.Dispose();
});

app.Run();

public partial class Program { }
