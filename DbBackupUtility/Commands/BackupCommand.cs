using System.CommandLine;
using DbBackupUtility.Models;
using DbBackupUtility.Providers;

namespace DbBackupUtility.Commands;

public static class BackupCommand
{
    public static Command Create()
    {
        var providerOption = new Option<string>("--provider", () => "postgres", "Database provider (currently: postgres)");
        var hostOption = new Option<string>("--host", "Database host") { IsRequired = true };
        var portOption = new Option<int>("--port", () => 5433, "Database port");
        var userOption = new Option<string>("--user", "Database username") { IsRequired = true };
        var passwordOption = new Option<string>("--password", "Database password") { IsRequired = true };
        var databaseOption = new Option<string>("--database", "Database name") { IsRequired = true };
        var containerOption = new Option<string>("--container", () => "database-backup-utility-postgres-1", "Docker container name");
        var outputDirOption = new Option<string>("--output-dir", () => "Backups", "Backup output directory");
        var fileOption = new Option<string?>("--file", "Full output file path for backup");

        var command = new Command("backup", "Create a database backup");
        command.AddOption(providerOption);
        command.AddOption(hostOption);
        command.AddOption(portOption);
        command.AddOption(userOption);
        command.AddOption(passwordOption);
        command.AddOption(databaseOption);
        command.AddOption(containerOption);
        command.AddOption(outputDirOption);
        command.AddOption(fileOption);

        command.SetHandler(async (provider, host, port, user, password, database, container, outputDir, file) =>
        {
            try
            {
                var connectionInfo = new DatabaseConnectionInfo(host, port, database, user, password);
                var databaseProvider = DatabaseProviderFactory.CreateProvider(provider, connectionInfo, container);

                var backupFilePath = BuildBackupPath(database, outputDir, file);
                var backupDirectory = Path.GetDirectoryName(backupFilePath);
                if (!string.IsNullOrWhiteSpace(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                Console.WriteLine($"Creating backup for '{database}'...");
                await databaseProvider.BackupDatabaseAsync(backupFilePath);
                Console.WriteLine($"Backup file saved at: {backupFilePath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Backup failed: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }, providerOption, hostOption, portOption, userOption, passwordOption, databaseOption, containerOption, outputDirOption, fileOption);

        return command;
    }

    private static string BuildBackupPath(string databaseName, string outputDir, string? filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            return filePath;
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(outputDir, $"{databaseName}_{timestamp}.backup");
    }
}