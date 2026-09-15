using Xunit;

namespace AnimalShelter.Tests.Integration;

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection
    : ICollectionFixture<AnimalShelterWebApplicationFactory>
{
}