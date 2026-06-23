using Serilog;

namespace DbBackupUtility.Services;

public static class LoggingService
{
    public static void Initialize()
    {
        Directory.CreateDirectory("Logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("Logs/dbbackup.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static void Close()
    {
        Log.CloseAndFlush();
    }
}