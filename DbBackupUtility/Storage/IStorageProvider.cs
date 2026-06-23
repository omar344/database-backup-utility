namespace DbBackupUtility.Storage
{
    public interface IStorageProvider
    {
        Task UploadAsync(string localPath, string remotePath);
        Task DownloadAsync(string remotePath, string localPath);
    }
}
