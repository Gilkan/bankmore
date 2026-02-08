using System.Net;
using Xunit;
using BankMore.Tests.Infrastructure;

namespace BankMore.Tests.Api;

public sealed class AuthTests
    : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AuthTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Endpoint_protegido_sem_token_deve_retornar_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/saldo");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
