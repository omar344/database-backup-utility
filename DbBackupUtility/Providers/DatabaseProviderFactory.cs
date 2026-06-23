using DbBackupUtility.Models;
using DbBackupUtility.Providers;

namespace DbBackupUtility.Providers
{
    public static class DatabaseProviderFactory
    {
        public static IDatabaseProvider Create(string provider, DatabaseConnectionInfo connectionInfo, string? containerName = null)
        {
            return provider.ToLower() switch
            {
                "postgres" or "postgresql" => new PostgreSqlProvider(connectionInfo, containerName),
                "mysql"                    => new MySqlProvider(connectionInfo, containerName),
                "mongo" or "mongodb"       => new MongoDbProvider(connectionInfo, containerName),
                "sqlite"                   => new SqliteProvider(connectionInfo),
                _ => throw new NotSupportedException($"Provider '{provider}' is not supported. Supported providers: postgres, mysql, mongo, sqlite.")
            };
        }
    }
}
