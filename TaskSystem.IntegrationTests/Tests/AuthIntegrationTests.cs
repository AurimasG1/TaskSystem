using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskSystem.Application.DTO.Requests.Auth;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.IntegrationTests.Infrastructure;

namespace TaskSystem.IntegrationTests.Tests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthIntegrationTests
{
    private const string Password = "IntegrationTest123!";

    private readonly HttpClient _client;

    public AuthIntegrationTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Register_ThenLogin_ReturnsTokens()
    {
        string email = CreateUniqueEmail();

        HttpResponseMessage registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new AuthRegisterRequest(email, Password)
        );

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new AuthLoginRequest(email, Password)
        );

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        AuthLoginResponse? login =
            await loginResponse.Content.ReadFromJsonAsync<AuthLoginResponse>();

        Assert.NotNull(login);

        Assert.Equal(email, login.Email);

        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));

        Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));
    }

    [Fact]
    public async Task Login_ThenUserMe_ReturnsAuthenticatedUser()
    {
        string email = CreateUniqueEmail();

        await RegisterAsync(email, Password);

        AuthLoginResponse login = await LoginAsync(email, Password);

        using HttpRequestMessage request = new(HttpMethod.Get, "/api/user/me");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_RotatesRefreshToken()
    {
        string email = CreateUniqueEmail();

        await RegisterAsync(email, Password);

        AuthLoginResponse login = await LoginAsync(email, Password);

        HttpResponseMessage refreshResponse = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(login.RefreshToken)
        );

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        AuthRefreshTokenResponse? refreshed =
            await refreshResponse.Content.ReadFromJsonAsync<AuthRefreshTokenResponse>();

        Assert.NotNull(refreshed);

        Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));

        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));

        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task ReusingOldRefreshToken_ReturnsUnauthorized()
    {
        string email = CreateUniqueEmail();

        await RegisterAsync(email, Password);

        AuthLoginResponse login = await LoginAsync(email, Password);

        RefreshTokenRequest request = new(login.RefreshToken);

        HttpResponseMessage firstRefresh = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            request
        );

        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);

        HttpResponseMessage reusedRefresh = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            request
        );

        Assert.Equal(HttpStatusCode.Unauthorized, reusedRefresh.StatusCode);
    }

    private async Task RegisterAsync(string email, string password)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new AuthRegisterRequest(email, password)
        );

        response.EnsureSuccessStatusCode();
    }

    private async Task<AuthLoginResponse> LoginAsync(string email, string password)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new AuthLoginRequest(email, password)
        );

        response.EnsureSuccessStatusCode();

        AuthLoginResponse? login = await response.Content.ReadFromJsonAsync<AuthLoginResponse>();

        return login ?? throw new InvalidOperationException("Login response body was empty");
    }

    private static string CreateUniqueEmail()
    {
        return $"integration-{Guid.NewGuid():N}" + "@test.local";
    }
}
