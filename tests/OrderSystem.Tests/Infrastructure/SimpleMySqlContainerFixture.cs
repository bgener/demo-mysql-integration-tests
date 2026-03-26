using System.Globalization;
using MySqlConnector;
using Testcontainers.MySql;
using Xunit;

namespace OrderSystem.Tests.Infrastructure;

public class SimpleMySqlContainerFixture : IAsyncLifetime, IMySqlContainerFixture
{
    private MySqlContainer _container = null!;
    public string RootConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .Build();

        await _container.StartAsync();
        RootConnectionString = _container.GetConnectionString();

        // Grant full privileges to the default 'mysql' user so it can create databases
        await _container.ExecAsync([
            "mysql", "-uroot",
            "-e", "GRANT ALL PRIVILEGES ON *.* TO 'mysql'@'%' WITH GRANT OPTION; FLUSH PRIVILEGES;"
        ]);
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    // Each test class gets a fresh database with DbUp migrations applied
    public async Task CreateDatabaseAsync(string databaseName, string connectionString)
    {
        await _container.ExecAsync([
            "mysql", "-uroot",
            "-e", $"CREATE DATABASE `{databaseName}`;"
        ]);

        MigrationRunner.Run(connectionString);
    }

    // Query via Docker exec: runs mysql CLI inside the container
    public async Task<T?> QueryScalarAsync<T>(string connectionString, string sql)
    {
        var db = new MySqlConnectionStringBuilder(connectionString).Database;
        var result = await _container.ExecAsync(["mysql", "-uroot", "-N", "-B", db, "-e", sql]);
        var value = result.Stdout.Trim();
        if (string.IsNullOrEmpty(value) || value == "NULL")
            return default;
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }
}
