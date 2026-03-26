using DbUp;

namespace OrderSystem;

public static class MigrationRunner
{
    public static void Run(string connectionString)
    {
        var safeConnectionString = connectionString + ";SslMode=None;AllowPublicKeyRetrieval=True";

        var upgrader = DeployChanges.To
            .MySqlDatabase(safeConnectionString)
            .WithScriptsEmbeddedInAssembly(typeof(MigrationRunner).Assembly)
            .LogToNowhere()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
            throw new Exception("Database migration failed", result.Error);
    }
}
