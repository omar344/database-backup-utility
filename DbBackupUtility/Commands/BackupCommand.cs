using System.CommandLine;
using DbBackupUtility.Models;
using DbBackupUtility.Providers;
using DbBackupUtility.Services;
using DbBackupUtility.Storage;
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
            var outputOption = new Option<string?>("--output", () => null, "Optional. Local path to store the temp/final backup file. Generates one if omitted.");
            var typeOption = new Option<BackupType>("--type", () => BackupType.Full, "Type of backup to perform: Full, Incremental, or Differential.");
            var compressOption = new Option<bool>("--compress", () => false, "Compress the final backup output to .zip.");
            
            var storageOption = new Option<string>("--storage", () => "local", "Storage backend provider: local, s3, azure, gcs");
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
            AddOption(outputOption);
            AddOption(typeOption);
            AddOption(compressOption);
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
                var output = context.ParseResult.GetValueForOption(outputOption);
                var type = context.ParseResult.GetValueForOption(typeOption);
                var compress = context.ParseResult.GetValueForOption(compressOption);
                var storage = context.ParseResult.GetValueForOption(storageOption)!;
                var webhook = context.ParseResult.GetValueForOption(webhookOption);

                LoggingService.LogInformation($"Starting {type} backup for {provider} database '{database}' on {host}:{port}...");
                NotificationService.SendSlackNotificationAsync(webhook, $"🚀 Starting {type} backup for {provider} database '{database}'...").Wait();
                
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
                    NotificationService.SendSlackNotificationAsync(webhook, $" Backup failed initialization: {ex.Message}").Wait();
                    return Task.CompletedTask;
                }

                string localBackupPath = Path.Combine(Path.GetTempPath(), $"{database}_{DateTime.Now:yyyyMMddHHmmss}.backup");
                if (storage == "local")
                {
                    localBackupPath = output ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups", $"{database}_{DateTime.Now:yyyyMMddHHmmss}.backup");
                    string? dir = Path.GetDirectoryName(localBackupPath);
                    if (dir != null && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                try
                {
                    dbProvider.BackupDatabaseAsync(localBackupPath, type).Wait();
                    
                    if (compress)
                    {
                        var compressor = new CompressionService();
                        localBackupPath = compressor.CompressAsync(localBackupPath).Result;
                    }

                    string remotePath = output ?? Path.GetFileName(localBackupPath);
                    storageService.UploadAsync(localBackupPath, remotePath).Wait();
                    
                    LoggingService.LogInformation($"Backup completed successfully and stored to {storage} at: {remotePath}");
                    NotificationService.SendSlackNotificationAsync(webhook, $" Backup completed successfully! Saved to {storage} at `{remotePath}`. Compress: {compress}").Wait();
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Backup failed.", ex);
                    NotificationService.SendSlackNotificationAsync(webhook, $"Backup failed: {ex.Message}").Wait();
                }
                
                return Task.CompletedTask;
            });
        }
    }
}
