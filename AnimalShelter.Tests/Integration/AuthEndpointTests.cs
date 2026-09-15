using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AnimalShelter.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AnimalShelter.Tests.Integration;

public class AuthEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnJwtToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        var login = new LoginDto
        {
            Username = "bissa",
            Password = "mypassword123"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/login",
            login);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.True(
            body.TryGetProperty("token", out var token));

        Assert.False(
            string.IsNullOrWhiteSpace(token.GetString()));
    }

    [Fact]
    public async Task GetAnimals_WithValidToken_ShouldReturn200()
    {
        // Arrange
        var client = _factory.CreateClient();

        var login = new LoginDto
        {
            Username = "bissa",
            Password = "mypassword123"
        };

        var loginResponse = await client.PostAsJsonAsync(
            "/login",
            login);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginBody =
            await loginResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        var token = loginBody
            .GetProperty("token")
            .GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        // Act
        var response = await client.GetAsync("/animals");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}   