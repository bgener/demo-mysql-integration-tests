namespace OrderSystem.Tests.Infrastructure;

public interface IMySqlContainerFixture
{
    string RootConnectionString { get; }
    Task CreateDatabaseAsync(string databaseName, string connectionString);
    Task<T?> QueryScalarAsync<T>(string connectionString, string sql);
}
