namespace DbBackupUtility.Providers
{
    public interface IDatabaseProvider
    {
        Task<bool> TestConnectionAsync();

        Task BackupDatabaseAsync(string backupFilePath);
    }
}