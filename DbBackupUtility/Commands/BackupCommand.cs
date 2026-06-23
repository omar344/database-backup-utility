using System.CommandLine;
using DbBackupUtility.Models;
using DbBackupUtility.Providers;
using DbBackupUtility.Services;
using System.IO;
using System.Threading.Tasks;
using System;

namespace DbBackupUtility.Commands
{
    public class BackupCommand : Command
    {
        public BackupCommand() : base("backup", "Create a database backup")
        {
            var providerOption = new Option<string>("--provider", "Database provider (e.g., postgres)") { IsRequired = true };
            var hostOption = new Option<string>("--host", () => "localhost", "Database host");
            var portOption = new Option<int>("--port", () => 5432, "Database port");
            var userOption = new Option<string>("--user", "Database user") { IsRequired = true };
            var passwordOption = new Option<string>("--password", "Database password") { IsRequired = true };
            var databaseOption = new Option<string>("--database", "Database name") { IsRequired = true };
            var outputOption = new Option<string?>("--output", () => null, "Optional. Path to store the backup file. If not provided, it generates one in the Backups folder.");

            AddOption(providerOption);
            AddOption(hostOption);
            AddOption(portOption);
            AddOption(userOption);
            AddOption(passwordOption);
            AddOption(databaseOption);
            AddOption(outputOption);

            this.SetHandler(async (string provider, string host, int port, string user, string password, string database, string? output) =>
            {
                LoggingService.LogInformation($"Starting backup for {provider} database '{database}' on {host}:{port}...");
                
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

                string backupPath = output ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups", $"{database}_{DateTime.Now:yyyyMMddHHmmss}.backup");

                // Ensure the Backups directory exists if we are using the default path
                string? dir = Path.GetDirectoryName(backupPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                try
                {
                    await dbProvider.BackupDatabaseAsync(backupPath);
                    LoggingService.LogInformation($"Backup completed successfully. Saved to: {backupPath}");
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Backup failed.", ex);
                }
                
            }, providerOption, hostOption, portOption, userOption, passwordOption, databaseOption, outputOption);
        }
    }
}
