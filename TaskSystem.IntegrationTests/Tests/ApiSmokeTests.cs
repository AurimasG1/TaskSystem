using System.Net;
using TaskSystem.IntegrationTests.Infrastructure;

namespace TaskSystem.IntegrationTests.Tests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ApiSmokeTests
{
    private readonly HttpClient _client;

    public ApiSmokeTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UserMe_WithoutAccessToken_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
