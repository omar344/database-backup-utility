namespace DbBackupUtility.Storage
{
    public static class StorageProviderFactory
    {
        public static IStorageProvider Create(string storageType, string? s3Bucket, string? s3AccessKey, string? s3SecretKey, string? s3Region, string? azureConn, string? azureContainer, string? gcsBucket, string? gcsKeyPath)
        {
            return storageType.ToLower() switch
            {
                "local" => new LocalStorageProvider(),
                "s3" => new S3StorageProvider(
                    s3Bucket ?? throw new ArgumentException("S3 bucket is required for S3 storage."),
                    s3AccessKey ?? throw new ArgumentException("S3 access key is required."),
                    s3SecretKey ?? throw new ArgumentException("S3 secret key is required."),
                    s3Region ?? "us-east-1"
                ),
                "azure" => new AzureBlobStorageProvider(
                    azureConn ?? throw new ArgumentException("Azure connection string is required."),
                    azureContainer ?? throw new ArgumentException("Azure container name is required.")
                ),
                "gcs" => new GoogleCloudStorageProvider(
                    gcsBucket ?? throw new ArgumentException("GCS bucket is required."),
                    gcsKeyPath
                ),
                _ => throw new NotSupportedException($"Storage type '{storageType}' is not supported. Supported: local, s3, azure, gcs.")
            };
        }
    }
}
