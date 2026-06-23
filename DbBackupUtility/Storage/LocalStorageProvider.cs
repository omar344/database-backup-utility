using DbBackupUtility.Services;
using System.IO;

namespace DbBackupUtility.Storage
{
    public class LocalStorageProvider : IStorageProvider
    {
        public Task UploadAsync(string localPath, string remotePath)
        {
            LoggingService.LogInformation($"[Local Storage] Storing backup to {remotePath}");
            string? dir = Path.GetDirectoryName(remotePath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (localPath != remotePath)
            {
                File.Copy(localPath, remotePath, true);
            }
            return Task.CompletedTask;
        }

        public Task DownloadAsync(string remotePath, string localPath)
        {
            LoggingService.LogInformation($"[Local Storage] Retrieving backup from {remotePath}");
            if (!File.Exists(remotePath))
            {
                throw new FileNotFoundException($"Storage file not found: {remotePath}");
            }
            if (localPath != remotePath)
            {
                File.Copy(remotePath, localPath, true);
            }
            return Task.CompletedTask;
        }
    }
}
