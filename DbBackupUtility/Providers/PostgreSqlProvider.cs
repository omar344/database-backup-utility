using System.Diagnostics;
using DbBackupUtility.Providers;
using DbBackupUtility.Models;

namespace DbBackupUtility.Providers
{
    public class PostgreSqlProvider : IDatabaseProvider
    {
        private readonly DatabaseConnectionInfo _connectionInfo;
        private readonly string _containerName;

        public string ProviderName => "PostgreSQL";

        public PostgreSqlProvider(DatabaseConnectionInfo connectionInfo, string? containerName = null)
        {
            _connectionInfo = connectionInfo;
            _containerName = string.IsNullOrWhiteSpace(containerName) ? "database-backup-utility-postgres-1" : containerName;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new Npgsql.NpgsqlConnection(
                    $"Host={_connectionInfo.Host};Port={_connectionInfo.Port};Username={_connectionInfo.Username};Password={_connectionInfo.Password};Database={_connectionInfo.DatabaseName}"))
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
                if (process == null) throw new Exception("Failed to start docker process.");
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
                if (copyProcess == null) throw new Exception("Failed to start docker cp process.");
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

        public async Task RestoreDatabaseAsync(string backupFilePath)
        {
            // 1. Copy backup file into container
            string copyArguments = $"cp \"{backupFilePath}\" {_containerName}:/tmp/restore.sql";
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
                if (copyProcess == null) throw new Exception("Failed to start docker cp process.");
                copyProcess.Start();
                await copyProcess.WaitForExitAsync();

                if (copyProcess.ExitCode != 0)
                {
                    string error = await copyProcess.StandardError.ReadToEndAsync();
                    throw new Exception($"docker cp failed with exit code {copyProcess.ExitCode}: {error}");
                }
            }

            // 2. Execute pg_restore inside container
            // Use --clean to drop existing objects before restoring
            string arguments = $"exec {_containerName} pg_restore -U {_connectionInfo.Username} -d {_connectionInfo.DatabaseName} --clean /tmp/restore.sql";

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
                if (process == null) throw new Exception("Failed to start docker process.");
                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    string error = await process.StandardError.ReadToEndAsync();
                    throw new Exception($"pg_restore failed with exit code {process.ExitCode}: {error}");
                }
            }

            Console.WriteLine($"Restore successful: {backupFilePath}");
        }
    }
}

    