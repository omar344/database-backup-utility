using Amazon.S3;
using Amazon.S3.Transfer;
using DbBackupUtility.Services;

namespace DbBackupUtility.Storage
{
    public class S3StorageProvider : IStorageProvider
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        public S3StorageProvider(string bucketName, string accessKey, string secretKey, string region)
        {
            _bucketName = bucketName;
            var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
            _s3Client = new AmazonS3Client(accessKey, secretKey, regionEndpoint);
        }

        public async Task UploadAsync(string localPath, string remotePath)
        {
            LoggingService.LogInformation($"[AWS S3] Uploading {localPath} to bucket '{_bucketName}' at '{remotePath}'");
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(localPath, _bucketName, remotePath);
            LoggingService.LogInformation($"[AWS S3] Upload complete.");
        }

        public async Task DownloadAsync(string remotePath, string localPath)
        {
            LoggingService.LogInformation($"[AWS S3] Downloading '{remotePath}' from bucket '{_bucketName}' to '{localPath}'");
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.DownloadAsync(localPath, _bucketName, remotePath);
            LoggingService.LogInformation($"[AWS S3] Download complete.");
        }
    }
}
