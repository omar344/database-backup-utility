using System.Threading.Tasks;
using DbBackupUtility.Models;

namespace DbBackupUtility.Providers
{
    public interface IDatabaseProvider
    {
        string ProviderName { get; }

        Task<bool> TestConnectionAsync();

        Task BackupDatabaseAsync(string backupFilePath, BackupType type = BackupType.Full);

        Task RestoreDatabaseAsync(string backupFilePath);
    }
}