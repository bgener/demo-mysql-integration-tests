using Dapper;
using MySqlConnector;
using Xunit;

namespace OrderSystem.Tests.Infrastructure;

public abstract class MySqlTestBase(IMySqlContainerFixture fixture) : IAsyncLifetime
{
    private string _databaseName = null!;
    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _databaseName = $"db_{Guid.NewGuid():N}"[..30];

        ConnectionString = new MySqlConnectionStringBuilder(fixture.RootConnectionString)
        {
            Database = _databaseName
        }.ConnectionString;

        await fixture.CreateDatabaseAsync(_databaseName, ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await using var conn = new MySqlConnection(fixture.RootConnectionString);
        await conn.ExecuteAsync($"DROP DATABASE IF EXISTS `{_databaseName}`;");
    }

    protected Task<T?> QueryScalarAsync<T>(string sql)
        => fixture.QueryScalarAsync<T>(ConnectionString, sql);

    protected async Task<int> SeedProductAsync(string name, decimal price, int stock)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Products (name, price, stock) VALUES (@name, @price, @stock);
              SELECT LAST_INSERT_ID();",
            new { name, price, stock });
    }

    protected async Task<int> SeedCustomerAsync(string name, string email)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Customers (name, email) VALUES (@name, @email);
              SELECT LAST_INSERT_ID();",
            new { name, email });
    }
}
