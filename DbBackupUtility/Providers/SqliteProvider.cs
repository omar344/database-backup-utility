using System.Diagnostics;
using DbBackupUtility.Models;
using Microsoft.Data.Sqlite;
using System.IO;

namespace DbBackupUtility.Providers
{
    public class SqliteProvider : IDatabaseProvider
    {
        private readonly DatabaseConnectionInfo _connectionInfo;

        public string ProviderName => "SQLite";

        public SqliteProvider(DatabaseConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                string filePath = _connectionInfo.DatabaseName;
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Connection test failed: SQLite database file '{filePath}' does not exist.");
                    return false;
                }

                string connectionString = $"Data Source={filePath};Mode=ReadOnly";
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync();
                
                Console.WriteLine("Connection test successful!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        public Task BackupDatabaseAsync(string backupFilePath, BackupType type = BackupType.Full)
        {
            if (type != BackupType.Full)
                throw new NotSupportedException($"BackupType {type} is currently not supported for {ProviderName}.");

            try
            {
                string sourceFilePath = _connectionInfo.DatabaseName;
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"SQLite source database file not found: {sourceFilePath}");
                }

                File.Copy(sourceFilePath, backupFilePath, true);
                Console.WriteLine($"Backup successful: {backupFilePath}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"SQLite backup failed: {ex.Message}", ex);
            }
        }

        public Task RestoreDatabaseAsync(string backupFilePath)
        {
            try
            {
                string targetFilePath = _connectionInfo.DatabaseName;
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException($"SQLite backup file not found: {backupFilePath}");
                }

                File.Copy(backupFilePath, targetFilePath, true);
                Console.WriteLine($"Restore successful: {backupFilePath}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"SQLite restore failed: {ex.Message}", ex);
            }
        }
    }
}
