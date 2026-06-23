using System.CommandLine;
using DbBackupUtility.Commands;
using DbBackupUtility.Services;

LoggingService.Initialize();

var rootCommand = new RootCommand("Database Backup Utility - cross-platform PostgreSQL backup and restore CLI")
{
    TestCommand.Create(),
    BackupCommand.Create(),
    RestoreCommand.Create()
};

try
{
    return await rootCommand.InvokeAsync(args);
}
finally
{
    LoggingService.Close();
}
