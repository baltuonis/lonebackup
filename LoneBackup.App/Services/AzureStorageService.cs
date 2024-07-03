using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;

namespace LoneBackup.App.Services;

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

        Console.WriteLine("Uploading to Azure Storage: " + filePath);
        var blobClient = _client.GetBlobClient(filePath);
        await blobClient.UploadAsync(zippedFileStream);
        Console.WriteLine("Upload complete");
    }

    private BlobContainerClient CreateStorageClient()
    {
        var blobClientOptions = new BlobClientOptions
        {
            RetryPolicy = new RetryPolicy(3),
        };
        
        // TODO: check
        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        var blobSvc = new BlobServiceClient(_appConfig.AzureConnectionString, blobClientOptions);
        var containerClient = blobSvc.GetBlobContainerClient(_appConfig.AzureContainer);
        
        Console.WriteLine($"Azure Storage: {blobSvc.Uri}");
        Console.WriteLine($"Blob Container: {_appConfig.AzureContainer}");

        containerClient.CreateIfNotExists();
        
        return containerClient;
    }
}