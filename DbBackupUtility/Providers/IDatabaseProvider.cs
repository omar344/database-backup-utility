namespace DbBackupUtility.Providers
{
    public interface IDatabaseProvider
    {
        string ProviderName { get; }

        Task<bool> TestConnectionAsync();

        Task BackupDatabaseAsync(string backupFilePath);

        Task RestoreDatabaseAsync(string backupFilePath);
    }
}