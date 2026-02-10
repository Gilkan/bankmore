using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using BankMore.Tests.Infrastructure;
using BankMore.Domain.ValueObjects;
using System.IO;

namespace BankMore.Tests.Api;

public sealed class TransferHttpIntegrationTests
    : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly string _logFile = Path.Combine(AppContext.BaseDirectory, "transfer_test_log.txt");

    // Toggle for debug logging
    private readonly bool _enableLogging = true;

    // Flag to disable Kafka for tests
    private readonly bool _disableKafka = true;

    public TransferHttpIntegrationTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();

        if (_enableLogging && File.Exists(_logFile))
            File.Delete(_logFile);
    }

    [Fact]
    public async Task Transferencia_autenticada_funciona_end_to_end()
    {
        // ---------- Arrange: create two valid accounts ----------
        var resp1 = await CriarContaAsync(1000, "1234");
        resp1.EnsureSuccessStatusCode();
        var resp2 = await CriarContaAsync(1001, "1234");
        resp2.EnsureSuccessStatusCode();

        // Login to get JWT
        var token = await LoginAsync(1000, "1234");

        if (_enableLogging)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                File.AppendAllText(_logFile, "JWT Claims:\n");
                foreach (var c in jwtToken.Claims)
                    File.AppendAllText(_logFile, $"  {c.Type} = {c.Value}\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(_logFile, $"Failed to read JWT token: {ex}\n");
            }
        }

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // ---------- Seed account balance ----------
        var movResp = await MovimentarAsync(1000, 200);
        movResp.EnsureSuccessStatusCode();
        if (_enableLogging) File.AppendAllText(_logFile, "Seeded 200 into account 1000\n");

        // ---------- Act: make a transfer ----------
        HttpResponseMessage transferResponse = null!;
        try
        {
            transferResponse = await _client.PostAsJsonAsync(
                "/api/transferencias",
                new
                {
                    IdentificacaoRequisicao = "http-int-1",
                    NumeroContaDestino = 1001,
                    Valor = 50
                });

            transferResponse.EnsureSuccessStatusCode();

            if (_enableLogging)
                File.AppendAllText(_logFile, "Transfer request succeeded\n");
        }
        catch (HttpRequestException)
        {
            if (_enableLogging && transferResponse != null)
            {
                var content = await transferResponse.Content.ReadAsStringAsync();
                File.AppendAllText(_logFile, "\nTRANSFER RESPONSE BODY:\n" + content + "\n");
            }
            throw;
        }

        // ---------- Assert HTTP response ----------
        Assert.Equal(HttpStatusCode.NoContent, transferResponse.StatusCode);

        // ---------- Kafka section skipped ----------
        if (!_disableKafka)
        {
            // Kafka assertions would go here
        }
        else if (_enableLogging)
        {
            File.AppendAllText(_logFile, "Kafka part skipped for this test\n");
        }

        if (_enableLogging) File.AppendAllText(_logFile, "Test completed\n");
    }

    // ---------- Helpers ----------

    private Task<HttpResponseMessage> CriarContaAsync(int numero, string senha)
    {
        if (_enableLogging) File.AppendAllText(_logFile, $"Creating account {numero}\n");
        return _client.PostAsJsonAsync("/api/contas", new
        {
            Nome = $"Conta {numero}",
            Cpf = Cpf.GenerateRandomCpfString(),
            Senha = senha
        });
    }

    private async Task<string> LoginAsync(int numero, string senha)
    {
        if (_enableLogging) File.AppendAllText(_logFile, $"Logging in account {numero}\n");
        var response = await _client.PostAsJsonAsync(
            "/api/login",
            new { NumeroConta = numero, Senha = senha });

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return json!.Token;
    }

    private Task<HttpResponseMessage> MovimentarAsync(int numero, decimal valor)
    {
        if (_enableLogging) File.AppendAllText(_logFile, $"Seeding movimentacao: account {numero} valor {valor}\n");
        return _client.PostAsJsonAsync("/api/movimentacoes", new
        {
            IdentificacaoRequisicao = $"seed-{numero}",
            Valor = valor,
            Tipo = "C"
        });
    }

    private sealed record LoginResponse(string Token);
}
