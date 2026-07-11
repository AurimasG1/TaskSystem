using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskSystem.Application.DTO.Requests.Auth;
using TaskSystem.Application.DTO.Requests.Users;
using TaskSystem.Application.DTO.Requests.Uzduotys;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.IntegrationTests.Infrastructure;

namespace TaskSystem.IntegrationTests.Tests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthorizationIntegrationTests
{
    private const string Password = "IntegrationTest123!";

    private readonly HttpClient _client;

    public AuthorizationIntegrationTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task NormalUser_AdminEndpoint_ReturnsForbidden()
    {
        AuthLoginResponse user = await RegisterCompletedUserAsync();

        using HttpRequestMessage request = CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/api/admin/users/1",
            user.AccessToken
        );

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserCannotAccessAnotherUsersTask()
    {
        AuthLoginResponse userA = await RegisterCompletedUserAsync();

        UzduotisDto task = await CreateTaskAsync(userA.AccessToken);

        AuthLoginResponse userB = await RegisterCompletedUserAsync();

        using HttpRequestMessage request = CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/api/user/uzduotys/{task.Id}",
            userB.AccessToken
        );

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<AuthLoginResponse> RegisterCompletedUserAsync()
    {
        string email = CreateUniqueEmail();

        HttpResponseMessage registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new AuthRegisterRequest(email, Password)
        );

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        AuthRegisterResponse? registered =
            await registerResponse.Content.ReadFromJsonAsync<AuthRegisterResponse>();

        Assert.NotNull(registered);

        using HttpRequestMessage profileRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            "/api/user/register-profile",
            registered.AccessToken
        );

        profileRequest.Content = JsonContent.Create(new UserRegisterRequest("Integration", "User"));

        HttpResponseMessage profileResponse = await _client.SendAsync(profileRequest);

        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        AuthLoginResponse? completedUser =
            await profileResponse.Content.ReadFromJsonAsync<AuthLoginResponse>();

        Assert.NotNull(completedUser);

        Assert.Equal("user", completedUser.Role);

        return completedUser;
    }

    private async Task<UzduotisDto> CreateTaskAsync(string accessToken)
    {
        using HttpRequestMessage request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            "/api/user/uzduotys",
            accessToken
        );

        request.Content = JsonContent.Create(
            new UzduotisCreateRequest(
                "User A private task",
                "This task must not be accessible by user B."
            )
        );

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        UzduotisDto? task = await response.Content.ReadFromJsonAsync<UzduotisDto>();

        return task ?? throw new InvalidOperationException("Create task response body was empty.");
    }

    private static HttpRequestMessage CreateAuthenticatedRequest(
        HttpMethod method,
        string requestUri,
        string accessToken
    )
    {
        HttpRequestMessage request = new(method, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }

    private static string CreateUniqueEmail()
    {
        return $"authorization-{Guid.NewGuid():N}" + "@test.local";
    }
}
