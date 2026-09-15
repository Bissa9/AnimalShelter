using Testcontainers.MySql;

namespace AnimalShelter.Tests.Integration;

public class MySqlContainerTests : IAsyncLifetime
{
    private readonly MySqlContainer _mysql =
        new MySqlBuilder("mysql:8.4")
            .WithDatabase("AnimalShelterTestDb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _mysql.DisposeAsync();
    }

    [Fact]
    public async Task Container_ShouldAcceptConnection()
    {
        var connectionString =
            _mysql.GetConnectionString();

        await using var connection =
            new MySqlConnector.MySqlConnection(
                connectionString);

        await connection.OpenAsync();

        Assert.Equal(
            System.Data.ConnectionState.Open,
            connection.State);
    }
}