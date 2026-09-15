using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AnimalShelter.Tests.Integration;

[Collection("Integration Tests")]
public class AnimalsEndpointTests
{
    private readonly AnimalShelterWebApplicationFactory _factory;

    public AnimalsEndpointTests(
        AnimalShelterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAnimals_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/animals");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}   