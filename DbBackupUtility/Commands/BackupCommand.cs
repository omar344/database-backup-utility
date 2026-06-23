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
            var typeOption = new Option<BackupType>("--type", () => BackupType.Full, "Type of backup to perform: Full, Incremental, or Differential.");
            var compressOption = new Option<bool>("--compress", () => false, "Compress the final backup output to .zip.");

            AddOption(providerOption);
            AddOption(hostOption);
            AddOption(portOption);
            AddOption(userOption);
            AddOption(passwordOption);
            AddOption(databaseOption);
            AddOption(outputOption);
            AddOption(typeOption);
            AddOption(compressOption);

            this.SetHandler((System.CommandLine.Invocation.InvocationContext context) =>
            {
                var provider = context.ParseResult.GetValueForOption(providerOption)!;
                var host = context.ParseResult.GetValueForOption(hostOption)!;
                var port = context.ParseResult.GetValueForOption(portOption);
                var user = context.ParseResult.GetValueForOption(userOption)!;
                var password = context.ParseResult.GetValueForOption(passwordOption)!;
                var database = context.ParseResult.GetValueForOption(databaseOption)!;
                var output = context.ParseResult.GetValueForOption(outputOption);
                var type = context.ParseResult.GetValueForOption(typeOption);
                var compress = context.ParseResult.GetValueForOption(compressOption);

                LoggingService.LogInformation($"Starting {type} backup for {provider} database '{database}' on {host}:{port}...");
                
                IDatabaseProvider dbProvider;
                try
                {
                    var connectionInfo = new DatabaseConnectionInfo(host, port, database, user, password);
                    dbProvider = DatabaseProviderFactory.Create(provider, connectionInfo);
                }
                catch (NotSupportedException ex)
                {
                    LoggingService.LogError(ex.Message);
                    return Task.CompletedTask;
                }

                string backupPath = output ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups", $"{database}_{DateTime.Now:yyyyMMddHHmmss}.backup");

                string? dir = Path.GetDirectoryName(backupPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                try
                {
                    dbProvider.BackupDatabaseAsync(backupPath, type).Wait();
                    
                    if (compress)
                    {
                        var compressor = new CompressionService();
                        backupPath = compressor.CompressAsync(backupPath).Result;
                    }
                    
                    LoggingService.LogInformation($"Backup completed successfully. Saved to: {backupPath}");
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Backup failed.", ex);
                }
                
                return Task.CompletedTask;
            });
        }
    }
}
