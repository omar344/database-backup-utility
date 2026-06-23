using System.CommandLine;
using System.IO;
using DbBackupUtility.Models;
using DbBackupUtility.Providers;
using DbBackupUtility.Services;
using System.Threading.Tasks;
using System;

namespace DbBackupUtility.Commands
{
    public class RestoreCommand : Command
    {
        public RestoreCommand() : base("restore", "Restore a database from a backup file")
        {
            var providerOption = new Option<string>("--provider", "Database provider (e.g., postgres)") { IsRequired = true };
            var hostOption = new Option<string>("--host", () => "localhost", "Database host");
            var portOption = new Option<int>("--port", () => 5432, "Database port");
            var userOption = new Option<string>("--user", "Database user") { IsRequired = true };
            var passwordOption = new Option<string>("--password", "Database password") { IsRequired = true };
            var databaseOption = new Option<string>("--database", "Database name") { IsRequired = true };
            var fileOption = new Option<string>("--file", "Path to the backup file to restore") { IsRequired = true };

            AddOption(providerOption);
            AddOption(hostOption);
            AddOption(portOption);
            AddOption(userOption);
            AddOption(passwordOption);
            AddOption(databaseOption);
            AddOption(fileOption);

            this.SetHandler(async (string provider, string host, int port, string user, string password, string database, string file) =>
            {
                LoggingService.LogInformation($"Starting restore for {provider} database '{database}' on {host}:{port} from file {file}...");
                
                if (!File.Exists(file))
                {
                    LoggingService.LogError($"Backup file not found at: {file}");
                    return;
                }

                IDatabaseProvider dbProvider;
                if (provider.ToLower() == "postgres")
                {
                    var connectionInfo = new DatabaseConnectionInfo(host, port, database, user, password);
                    dbProvider = new PostgreSqlProvider(connectionInfo);
                }
                else
                {
                    LoggingService.LogError($"Provider '{provider}' is not supported yet.");
                    return;
                }

                try
                {
                    await dbProvider.RestoreDatabaseAsync(file);
                    LoggingService.LogInformation($"Restore completed successfully from: {file}");
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Restore failed.", ex);
                }
            }, providerOption, hostOption, portOption, userOption, passwordOption, databaseOption, fileOption);
        }
    }
}
