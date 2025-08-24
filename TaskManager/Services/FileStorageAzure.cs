using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TaskManager.Models.Dtos;

namespace TaskManager.Services
{
    public class FileStorageAzure : IFileStorage
    {
        private readonly string connectionString;
        public FileStorageAzure(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureStorage")!;
        }
        public async Task Delete(string path, string container)
        {
            if (string.IsNullOrEmpty(path)) 
            {
                return;
            }

            var client = new BlobContainerClient(connectionString, container);
            await client.CreateIfNotExistsAsync();

            var fileName = Path.GetFileName(path);
            var blob = client.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<FileStorageResult[]> Store(string container, 
            IEnumerable<IFormFile> files)
        {
            var client = new BlobContainerClient(connectionString, container);
            await client.CreateIfNotExistsAsync();
            client.SetAccessPolicy(PublicAccessType.Blob);

            var tasks = files.Select(async file =>
            {
                var originalFileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var blob = client.GetBlobClient(fileName);
                var blobHttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = file.ContentType
                };
                await blob.UploadAsync(file.OpenReadStream(), blobHttpHeaders);
                return new FileStorageResult
                {
                    URL = blob.Uri.ToString(),
                    Title = originalFileName
                };
            });

            var results = await Task.WhenAll(tasks);
            return results;
        }
    }
}
