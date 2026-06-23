using DbBackupUtility.Services;
using Google.Cloud.Storage.V1;
using System.IO;

namespace DbBackupUtility.Storage
{
    public class GoogleCloudStorageProvider : IStorageProvider
    {
        private readonly string _bucketName;
        private readonly StorageClient _storageClient;

        public GoogleCloudStorageProvider(string bucketName, string? keyFilePath = null)
        {
            _bucketName = bucketName;
            
            if (!string.IsNullOrEmpty(keyFilePath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);
            }
            
            _storageClient = StorageClient.Create();
        }

        public async Task UploadAsync(string localPath, string remotePath)
        {
            LoggingService.LogInformation($"[GCS] Uploading {localPath} to bucket '{_bucketName}' at '{remotePath}'");
            using var fileStream = File.OpenRead(localPath);
            await _storageClient.UploadObjectAsync(_bucketName, remotePath, null, fileStream);
            LoggingService.LogInformation($"[GCS] Upload complete.");
        }

        public async Task DownloadAsync(string remotePath, string localPath)
        {
            LoggingService.LogInformation($"[GCS] Downloading '{remotePath}' from bucket '{_bucketName}' to '{localPath}'");
            using var fileStream = File.Create(localPath);
            await _storageClient.DownloadObjectAsync(_bucketName, remotePath, fileStream);
            LoggingService.LogInformation($"[GCS] Download complete.");
        }
    }
}
