using System.CommandLine;
using System.Threading.Tasks;
using DbBackupUtility.Commands;
using DbBackupUtility.Services;

namespace DbBackupUtility
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            LoggingService.Initialize();

            var rootCommand = new RootCommand("Database Backup Utility CLI")
            {
                new TestCommand(),
                new BackupCommand(),
                new RestoreCommand()
            };

            int result = await rootCommand.InvokeAsync(args);

            LoggingService.CloseAndFlush();
            return result;
        }
    }
}
