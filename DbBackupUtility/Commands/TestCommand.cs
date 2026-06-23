using System.CommandLine;
using DbBackupUtility.Models;
using DbBackupUtility.Providers;
using DbBackupUtility.Services;

namespace DbBackupUtility.Commands
{
    public class TestCommand : Command
    {
        public TestCommand() : base("test", "Test database connection")
        {
            var providerOption = new Option<string>("--provider", "Database provider (e.g., postgres)") { IsRequired = true };
            var hostOption = new Option<string>("--host", () => "localhost", "Database host");
            var portOption = new Option<int>("--port", () => 5432, "Database port");
            var userOption = new Option<string>("--user", "Database user") { IsRequired = true };
            var passwordOption = new Option<string>("--password", "Database password") { IsRequired = true };
            var databaseOption = new Option<string>("--database", "Database name") { IsRequired = true };

            AddOption(providerOption);
            AddOption(hostOption);
            AddOption(portOption);
            AddOption(userOption);
            AddOption(passwordOption);
            AddOption(databaseOption);

            this.SetHandler(async (string provider, string host, int port, string user, string password, string database) =>
            {
                LoggingService.LogInformation($"Testing connection to {provider} on {host}:{port} ({database})...");
                
                IDatabaseProvider dbProvider;
                try
                {
                    var connectionInfo = new DatabaseConnectionInfo(host, port, database, user, password);
                    dbProvider = DatabaseProviderFactory.Create(provider, connectionInfo);
                }
                catch (NotSupportedException ex)
                {
                    LoggingService.LogError(ex.Message);
                    return;
                }

                bool isConnected = await dbProvider.TestConnectionAsync();
                if (isConnected)
                {
                    LoggingService.LogInformation("Database connection successful.");
                }
                else
                {
                    LoggingService.LogError("Failed to connect to the database.");
                }
            }, providerOption, hostOption, portOption, userOption, passwordOption, databaseOption);
        }
    }
}
