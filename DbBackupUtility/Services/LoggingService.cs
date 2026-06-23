using Serilog;
using System;

namespace DbBackupUtility.Services
{
    public static class LoggingService
    {
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/dbbackup.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void LogInformation(string message)
        {
            Log.Information(message);
        }

        public static void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            if (ex != null)
                Log.Error(ex, message);
            else
                Log.Error(message);
        }

        public static void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }
    }
}
