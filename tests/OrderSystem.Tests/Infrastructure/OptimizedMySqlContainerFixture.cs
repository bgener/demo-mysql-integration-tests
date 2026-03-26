using System.Diagnostics;
using Dapper;
using MySqlConnector;
using Testcontainers.MySql;
using Xunit;

namespace OrderSystem.Tests.Infrastructure;

public class OptimizedMySqlContainerFixture : IAsyncLifetime, IMySqlContainerFixture
{
    private const string BaselineImage = "mysql-test-baseline:latest";
    private const string TemplateDatabase = "baseline";
    private MySqlContainer _container = null!;

    public string RootConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var useBaseline = await ImageExistsAsync(BaselineImage);

        _container = new MySqlBuilder()
            .WithImage(useBaseline ? BaselineImage : "mysql:8.0")
            .WithCommand(
                "--skip-log-bin",
                "--innodb-doublewrite=0",
                "--innodb-flush-log-at-trx-commit=0",
                "--performance-schema=OFF")
            .Build();

        await _container.StartAsync();
        RootConnectionString = _container.GetConnectionString();

        // Grant full privileges to the default 'mysql' user so it can create databases
        await _container.ExecAsync([
            "mysql", "-uroot",
            "-e", "GRANT ALL PRIVILEGES ON *.* TO 'mysql'@'%' WITH GRANT OPTION; FLUSH PRIVILEGES;"
        ]);

        if (!useBaseline)
        {
            // No baseline image: create template DB, run DbUp migrations, then dump
            await using var conn = new MySqlConnection(RootConnectionString);
            await conn.ExecuteAsync($"CREATE DATABASE `{TemplateDatabase}`;");

            var templateConnStr = new MySqlConnectionStringBuilder(RootConnectionString)
            {
                Database = TemplateDatabase
            }.ConnectionString;
            MigrationRunner.Run(templateConnStr);
            await DumpDatabaseAsync();
        }
        // When using the baseline image, /baseline.sql is already baked in
    }

    // Restore the cached dump into a fresh database per test class
    // Uses the dump file inside the container so the mysql CLI handles DELIMITER blocks
    public async Task CreateDatabaseAsync(string databaseName, string connectionString)
    {
        await _container.ExecAsync([
            "bash", "-c",
            $"mysql -uroot -e 'CREATE DATABASE `{databaseName}`;' && mysql -uroot {databaseName} < /baseline.sql"
        ]);
    }

    // Query via TCP: direct connection to the database
    public async Task<T?> QueryScalarAsync<T>(string connectionString, string sql)
    {
        await using var conn = new MySqlConnection(connectionString);
        return await conn.ExecuteScalarAsync<T>(sql);
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private async Task DumpDatabaseAsync()
    {
        await _container.ExecAsync([
            "bash", "-c",
            $"mysqldump -uroot --routines --triggers {TemplateDatabase} > /baseline.sql"
        ]);
    }

    private static async Task<bool> ImageExistsAsync(string imageName)
    {
        try
        {
            var psi = new ProcessStartInfo("docker", $"image inspect {imageName}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var process = Process.Start(psi);
            if (process is null) return false;
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch { return false; }
    }
}
