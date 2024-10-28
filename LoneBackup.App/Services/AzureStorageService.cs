using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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
        // TODO: make tier configurable
        await blobClient.UploadAsync(zippedFileStream, accessTier: AccessTier.Cool);
        Console.WriteLine("Upload complete");
    }

    public async Task TryDeleteOldArchivesAsync(CancellationToken ct)
    {
        var olderThanDays = _appConfig.DeleteOlderThanDays;
        if (olderThanDays <= 0)
        {
            Console.WriteLine("Skipping delete old archives");
            return;
        }

        Console.WriteLine($"Cleaning up archives older than {olderThanDays} days");

        var dateBefore = DateTime.UtcNow.AddDays(-olderThanDays);
        var deletedCount = 0;
        long deletedBytes = 0;

        await foreach (var blobItem in _client.GetBlobsAsync(cancellationToken: ct))
        {
            if (!(blobItem.Properties.LastModified < dateBefore)) continue;

            try
            {
                // TODO: could use batch client here https://learn.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs.batch-readme?view=azure-dotnet
                await _client.DeleteBlobAsync(blobItem.Name, cancellationToken: ct);
                deletedCount++;
                deletedBytes += blobItem.Properties.ContentLength ?? 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to delete blob {blobItem.Name}");
                Console.WriteLine(e);
            }
        }

        Console.WriteLine($"Deleted {deletedCount} ({deletedBytes / 1024 / 1024} MB) old archives");
    }

    private BlobContainerClient CreateStorageClient()
    {
        var blobClientOptions = new BlobClientOptions
        {
            RetryPolicy = new RetryPolicy(3),
        };

        var blobSvc = new BlobServiceClient(_appConfig.AzureConnectionString, blobClientOptions);
        var containerClient = blobSvc.GetBlobContainerClient(_appConfig.AzureContainer);

        Console.WriteLine($"Azure Storage: {blobSvc.Uri}");
        Console.WriteLine($"Blob Container: {_appConfig.AzureContainer}");

        containerClient.CreateIfNotExists();

        return containerClient;
    }
}