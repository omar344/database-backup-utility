using DbBackupUtility.Models;

namespace DbBackupUtility.Providers;

public static class DatabaseProviderFactory
{
    public static IDatabaseProvider CreateProvider(string provider, DatabaseConnectionInfo connectionInfo, string containerName)
    {
        if (provider.Equals("postgres", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
        {
            return new PostgreSqlProvider(connectionInfo, containerName);
        }

        throw new NotSupportedException($"Provider '{provider}' is not supported yet. Use 'postgres'.");
    }
}
