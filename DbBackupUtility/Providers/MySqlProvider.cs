using System.Diagnostics;
using DbBackupUtility.Models;
using MySqlConnector;

namespace DbBackupUtility.Providers
{
    public class MySqlProvider : IDatabaseProvider
    {
        private readonly DatabaseConnectionInfo _connectionInfo;
        private readonly string _containerName;

        public string ProviderName => "MySQL";

        public MySqlProvider(DatabaseConnectionInfo connectionInfo, string? containerName = null)
        {
            _connectionInfo = connectionInfo;
            _containerName = string.IsNullOrWhiteSpace(containerName) ? "database-backup-utility-mysql-1" : containerName;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                string connectionString = $"Server={_connectionInfo.Host};Port={_connectionInfo.Port};Database={_connectionInfo.DatabaseName};Uid={_connectionInfo.Username};Pwd={_connectionInfo.Password};";
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                Console.WriteLine("Connection test successful!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        public async Task BackupDatabaseAsync(string backupFilePath, BackupType type = BackupType.Full)
        {
            if (type != BackupType.Full)
                throw new NotSupportedException($"BackupType {type} is currently not supported for {ProviderName}.");

            string arguments = $"exec {_containerName} sh -c \"mysqldump -u{_connectionInfo.Username} -p{_connectionInfo.Password} {_connectionInfo.DatabaseName} > /tmp/backup.sql\"";

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
                    throw new Exception($"mysqldump failed with exit code {process.ExitCode}: {error}");
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

            string arguments = $"exec {_containerName} sh -c \"mysql -u{_connectionInfo.Username} -p{_connectionInfo.Password} {_connectionInfo.DatabaseName} < /tmp/restore.sql\"";

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
                    throw new Exception($"mysql restore failed with exit code {process.ExitCode}: {error}");
                }
            }

            Console.WriteLine($"Restore successful: {backupFilePath}");
        }
    }
}
