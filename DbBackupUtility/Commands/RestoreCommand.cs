using System.CommandLine;
using System.IO;
using DbBackupUtility.Models;
using DbBackupUtility.Providers;
using DbBackupUtility.Services;
using DbBackupUtility.Storage;
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
            var storageOption = new Option<string>("--storage", () => "local", "Storage backend provider: local, s3, azure, gcs");

            // Mock options
            var s3BucketOpt = new Option<string?>("--s3-bucket", () => null, "S3 bucket");
            var s3AccessOpt = new Option<string?>("--s3-access", () => null, "S3 access");
            var s3SecretOpt = new Option<string?>("--s3-secret", () => null, "S3 secret");
            var s3RegionOpt = new Option<string?>("--s3-region", () => "us-east-1", "S3 region");
            var azureConnOpt = new Option<string?>("--azure-conn", () => null, "Azure connection");
            var azureContOpt = new Option<string?>("--azure-container", () => null, "Azure container");
            var gcsBucketOpt = new Option<string?>("--gcs-bucket", () => null, "GCS bucket");
            var gcsKeyOpt = new Option<string?>("--gcs-key", () => null, "GCS key");

            AddOption(providerOption);
            AddOption(hostOption);
            AddOption(portOption);
            AddOption(userOption);
            AddOption(passwordOption);
            AddOption(databaseOption);
            AddOption(fileOption);
            AddOption(storageOption);
            
            AddOption(s3BucketOpt);
            AddOption(s3AccessOpt);
            AddOption(s3SecretOpt);
            AddOption(s3RegionOpt);
            AddOption(azureConnOpt);
            AddOption(azureContOpt);
            AddOption(gcsBucketOpt);
            AddOption(gcsKeyOpt);

            var webhookOption = new Option<string?>("--slack-webhook", () => null, "Optional webhook URL for Slack notifications.");
            AddOption(webhookOption);

            this.SetHandler((System.CommandLine.Invocation.InvocationContext context) =>
            {
                var provider = context.ParseResult.GetValueForOption(providerOption)!;
                var host = context.ParseResult.GetValueForOption(hostOption)!;
                var port = context.ParseResult.GetValueForOption(portOption);
                var user = context.ParseResult.GetValueForOption(userOption)!;
                var password = context.ParseResult.GetValueForOption(passwordOption)!;
                var database = context.ParseResult.GetValueForOption(databaseOption)!;
                var file = context.ParseResult.GetValueForOption(fileOption)!;
                var storage = context.ParseResult.GetValueForOption(storageOption)!;
                var webhook = context.ParseResult.GetValueForOption(webhookOption);

                LoggingService.LogInformation($"Starting restore for {provider} database '{database}' on {host}:{port} from {(storage == "local" ? "local file" : storage + " remote")} {file}...");
                NotificationService.SendSlackNotificationAsync(webhook, $"🔄 Starting restore for {provider} database '{database}' from {storage}...").Wait();
                
                string localFileToRestore = file;

                IStorageProvider storageService;
                IDatabaseProvider dbProvider;
                try
                {
                    storageService = StorageProviderFactory.Create(storage,
                        context.ParseResult.GetValueForOption(s3BucketOpt),
                        context.ParseResult.GetValueForOption(s3AccessOpt),
                        context.ParseResult.GetValueForOption(s3SecretOpt),
                        context.ParseResult.GetValueForOption(s3RegionOpt),
                        context.ParseResult.GetValueForOption(azureConnOpt),
                        context.ParseResult.GetValueForOption(azureContOpt),
                        context.ParseResult.GetValueForOption(gcsBucketOpt),
                        context.ParseResult.GetValueForOption(gcsKeyOpt));

                    var connectionInfo = new DatabaseConnectionInfo(host, port, database, user, password);
                    dbProvider = DatabaseProviderFactory.Create(provider, connectionInfo);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError(ex.Message);
                    NotificationService.SendSlackNotificationAsync(webhook, $"❌ Restore failed initialization: {ex.Message}").Wait();
                    return Task.CompletedTask;
                }

                try
                {
                    if (storage != "local")
                    {
                        localFileToRestore = Path.Combine(Path.GetTempPath(), Path.GetFileName(file));
                        storageService.DownloadAsync(file, localFileToRestore).Wait();
                    }
                    else if (!File.Exists(localFileToRestore))
                    {
                        LoggingService.LogError($"Backup file not found at: {localFileToRestore}");
                        NotificationService.SendSlackNotificationAsync(webhook, $"❌ Restore failed: Backup file not found at {localFileToRestore}").Wait();
                        return Task.CompletedTask;
                    }

                    dbProvider.RestoreDatabaseAsync(localFileToRestore).Wait();
                    LoggingService.LogInformation($"Restore completed successfully from: {file}");
                    NotificationService.SendSlackNotificationAsync(webhook, $"✅ Restore completed successfully for '{database}'!").Wait();
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Restore failed.", ex);
                    NotificationService.SendSlackNotificationAsync(webhook, $"❌ Restore failed: {ex.Message}").Wait();
                }
                
                return Task.CompletedTask;
            });
        }
    }
}
