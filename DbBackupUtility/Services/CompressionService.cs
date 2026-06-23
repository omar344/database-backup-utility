using System.IO.Compression;

namespace DbBackupUtility.Services
{
    public interface ICompressionService
    {
        Task<string> CompressAsync(string sourceFilePath);
    }

    public class CompressionService : ICompressionService
    {
        public async Task<string> CompressAsync(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Cannot compress file because it does not exist.", sourceFilePath);

            string archivePath = sourceFilePath + ".zip";

            LoggingService.LogInformation($"Compressing {sourceFilePath} to {archivePath}...");

            await Task.Run(() =>
            {
                using var archiveStream = new FileStream(archivePath, FileMode.Create);
                using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true);
                var zipArchiveEntry = archive.CreateEntry(Path.GetFileName(sourceFilePath), CompressionLevel.Optimal);

                using var entryStream = zipArchiveEntry.Open();
                using var fileToCompressStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);

                fileToCompressStream.CopyTo(entryStream);
            });

            // Optional: Remove original file after compression to save space
            File.Delete(sourceFilePath);

            LoggingService.LogInformation($"Compression completed. Original file removed. Kept: {archivePath}");
            return archivePath;
        }
    }
}
