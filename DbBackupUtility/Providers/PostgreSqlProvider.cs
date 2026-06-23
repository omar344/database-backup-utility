using System.Diagnostics;
using DbBackupUtility.Providers;

class PostgreSqlProvider : IDatabaseProvider
{    
    private readonly DatabaseConnectionInfo _connectionInfo;
    private readonly string _containerName;
    public PostgreSqlProvider( DatabaseConnectionInfo connectionInfo, string containerName)
    {
        _connectionInfo = connectionInfo;
        _containerName = string.IsNullOrWhiteSpace(containerName) ? "database-backup-utility-postgres-1" : containerName;
    }
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            dynamic connectionInfo = new DatabaseConnectionInfo("localhost", 5433, "testdb", "admin", "admin");
            using (var connection = new Npgsql.NpgsqlConnection(
                $"Host={connectionInfo.Host};Port={connectionInfo.Port};Username={connectionInfo.Username};Password={connectionInfo.Password};Database={connectionInfo.DatabaseName}"))
            {
                await connection.OpenAsync();
                Console.WriteLine("Connection test successful!");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection test failed: {ex.Message}");
            return false;
        }
    }

    public async Task BackupDatabaseAsync(string backupFilePath)
    {
        string arguments = $"exec {_containerName} pg_dump -U {_connectionInfo.Username} -d {_connectionInfo.DatabaseName} -F c -f /tmp/backup.sql";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };


        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"pg_dump failed with exit code {process.ExitCode}: {error}");
            }
        }

        string copyArguments = $"cp {_containerName}:/tmp/backup.sql \"{backupFilePath}\"";
        var copyProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = copyArguments,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (var copyProcess = new Process { StartInfo = copyProcessStartInfo })
        {
            copyProcess.Start();
            await copyProcess.WaitForExitAsync();

            if (copyProcess.ExitCode != 0)
            {
                string error = await copyProcess.StandardError.ReadToEndAsync();
                throw new Exception($"docker cp failed with exit code {copyProcess.ExitCode}: {error}");
            }
        }

        Console.WriteLine($"Backup successful: {backupFilePath}");
    }

}

    