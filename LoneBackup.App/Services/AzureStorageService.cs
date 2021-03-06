using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace LoneBackup.App.Services
{
    public class AzureStorageService
    {
        private readonly AppConfig _appConfig;
        private readonly BlobContainerClient _client;

        public AzureStorageService(AppConfig appConfig)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _client = CreateStorageClient();
        }

        public async Task UploadToStorage(Stream zippedFileStream, string filename)
        {
            var filePath = Path.Combine(_appConfig.AzureFolder, filename);

            Console.WriteLine("File path: " + filePath);
            Console.WriteLine("Upload: in progress...");
            var blobClient = _client.GetBlobClient(filePath);
            await blobClient.UploadAsync(zippedFileStream);
            Console.WriteLine("Upload: done ");
        }

        private BlobContainerClient CreateStorageClient()
        {
            var blobSvc = new BlobServiceClient(_appConfig.AzureConnectionString);
            var containerClient = blobSvc.GetBlobContainerClient(_appConfig.AzureContainer);
            Console.WriteLine($"Azure Storage: {blobSvc.Uri}");
            Console.WriteLine($"Blob Container: {_appConfig.AzureContainer}");
            return containerClient;
        }
    }
}