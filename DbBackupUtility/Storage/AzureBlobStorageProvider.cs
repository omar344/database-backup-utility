using Azure.Storage.Blobs;
using DbBackupUtility.Services;

namespace DbBackupUtility.Storage
{
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private readonly string _containerName;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageProvider(string connectionString, string containerName)
        {
            _containerName = containerName;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task UploadAsync(string localPath, string remotePath)
        {
            LoggingService.LogInformation($"[Azure Blob] Uploading {localPath} to container '{_containerName}' at '{remotePath}'");
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(remotePath);
            await blobClient.UploadAsync(localPath, true);
            LoggingService.LogInformation($"[Azure Blob] Upload complete.");
        }

        public async Task DownloadAsync(string remotePath, string localPath)
        {
            LoggingService.LogInformation($"[Azure Blob] Downloading '{remotePath}' from container '{_containerName}' to '{localPath}'");
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(remotePath);
            await blobClient.DownloadToAsync(localPath);
            LoggingService.LogInformation($"[Azure Blob] Download complete.");
        }
    }
}
