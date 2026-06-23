using System.CommandLine;
using DbBackupUtility.Models;
using DbBackupUtility.Providers;

namespace DbBackupUtility.Commands;

public static class RestoreCommand
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
        var fileOption = new Option<string>("--file", "Backup file path to restore") { IsRequired = true };

        var command = new Command("restore", "Restore a database backup");
        command.AddOption(providerOption);
        command.AddOption(hostOption);
        command.AddOption(portOption);
        command.AddOption(userOption);
        command.AddOption(passwordOption);
        command.AddOption(databaseOption);
        command.AddOption(containerOption);
        command.AddOption(fileOption);

        command.SetHandler(async (provider, host, port, user, password, database, container, filePath) =>
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Backup file not found: {filePath}");
                }

                var connectionInfo = new DatabaseConnectionInfo(host, port, database, user, password);
                var databaseProvider = DatabaseProviderFactory.CreateProvider(provider, connectionInfo, container);

                Console.WriteLine($"Restoring '{database}' from '{filePath}'...");
                await databaseProvider.RestoreDatabaseAsync(filePath);
                Console.WriteLine("Restore completed.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Restore failed: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }, providerOption, hostOption, portOption, userOption, passwordOption, databaseOption, containerOption, fileOption);

        return command;
    }
}