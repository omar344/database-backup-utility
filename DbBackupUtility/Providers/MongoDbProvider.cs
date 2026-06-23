using System.Diagnostics;
using DbBackupUtility.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DbBackupUtility.Providers
{
    public class MongoDbProvider : IDatabaseProvider
    {
        private readonly DatabaseConnectionInfo _connectionInfo;
        private readonly string _containerName;

        public string ProviderName => "MongoDB";

        public MongoDbProvider(DatabaseConnectionInfo connectionInfo, string? containerName = null)
        {
            _connectionInfo = connectionInfo;
            _containerName = string.IsNullOrWhiteSpace(containerName) ? "database-backup-utility-mongo-1" : containerName;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var credentials = string.IsNullOrEmpty(_connectionInfo.Username) ? "" : $"{_connectionInfo.Username}:{_connectionInfo.Password}@";
                string connectionString = $"mongodb://{credentials}{_connectionInfo.Host}:{_connectionInfo.Port}";
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(_connectionInfo.DatabaseName);
                await database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
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

            string arguments = $"exec {_containerName} sh -c \"mongodump --username {_connectionInfo.Username} --password {_connectionInfo.Password} --authenticationDatabase admin --db {_connectionInfo.DatabaseName} --archive=/tmp/backup.archive\"";

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
                    throw new Exception($"mongodump failed with exit code {process.ExitCode}: {error}");
                }
            }

            string copyArguments = $"cp {_containerName}:/tmp/backup.archive \"{backupFilePath}\"";
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
            string copyArguments = $"cp \"{backupFilePath}\" {_containerName}:/tmp/restore.archive";
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

            string arguments = $"exec {_containerName} sh -c \"mongorestore --username {_connectionInfo.Username} --password {_connectionInfo.Password} --authenticationDatabase admin --drop --archive=/tmp/restore.archive\"";

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
                    throw new Exception($"mongorestore failed with exit code {process.ExitCode}: {error}");
                }
            }

            Console.WriteLine($"Restore successful: {backupFilePath}");
        }
    }
}
