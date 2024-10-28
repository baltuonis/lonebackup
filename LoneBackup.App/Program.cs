using System;
using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using LoneBackup.App.Services;
using LoneBackup.App.Utils;
using ConfigurationBuilder = LoneBackup.App.Config.ConfigurationBuilder;

namespace LoneBackup.App;

public class Program
{
    private const string AppVersion = "1.0.3";

    // TODO: add dry run (check connections, don't upload/cleanup)
    // TODO: logger + generic host container
    // TODO: allow CTRL+C
    // https://github.com/natemcmaster/CommandLineUtils
    // https://darthpedro.net/2024/06/26/using-generichost-in-a-c-console-application/
    // https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service
    static async Task<int> Main(string[] args)
    {
        // https://darthpedro.net/2024/06/26/using-generichost-in-a-c-console-application/
        var configFileOption = new Option<string>(
            name: "-c",
            description: "Configuration file path (default: config.json)",
            getDefaultValue: () => "config.json");

        var rootCommand = new RootCommand("Simple MySQL/MariaDB backup utility");
        rootCommand.AddOption(configFileOption);

        rootCommand.SetHandler(async (configFile) =>
            {
                await OnExecuteAsync(configFile);
            },
            configFileOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static AppConfig _config;
    private static bool _uploading;

    public static async Task<int> OnExecuteAsync(string configFile)
    {
        var stopWatch = Stopwatch.StartNew();
        Console.WriteLine("LoneBackup v" + AppVersion);

        if (string.IsNullOrEmpty(configFile))
        {
            return 0;
        }

        var configuration = new ConfigurationBuilder(configFile);
        _config = configuration.Build();

        var mysqlService = new MySqlService(_config);
        var azureService = new AzureStorageService(_config);

        await azureService.TryDeleteOldArchivesAsync(CancellationToken.None);

        Console.WriteLine("Starting database(s) dump process");

        await using var zippedFileStream = new ObservableMemoryStream(UploadProgressCallback);
        await using var zipStream = new ZipOutputStream(zippedFileStream);

        zipStream.SetLevel(9);
        zipStream.Password = _config.ArchivePassword;

        var successfulDumpsCount = 0;
        // Add zip entries for each database
        foreach (var dbName in _config.Databases)
        {
            // Stream backupStream;
            var entry = new ZipEntry(ZipEntry.CleanName(dbName + ".sql")) { DateTime = DateTime.Now, Size = -1, };
            await zipStream.PutNextEntryAsync(entry);
            
            try
            {
                 mysqlService.GetDatabaseBackup(dbName, zipStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB: Failed access database `{dbName}`. Exception:");
                Console.WriteLine(ex);
                continue;
            }

            successfulDumpsCount++;
            zipStream.CloseEntry();
            
            Console.WriteLine($"DB: `{dbName}` dumped");
        }

        if (successfulDumpsCount == 0)
        {
            throw new Exception("Failed to access any of the provided databases");
        }

        // Zip file completed
        zipStream.Finish();
        zippedFileStream.Position = 0;

        var timestamp = DateTime.Now.ToString("s", DateTimeFormatInfo.InvariantInfo);
        var filename = $"db-backup-{timestamp}.zip";

        Console.WriteLine("Compression complete");
        
        if (_config.CreateLocalFile)
        {
            await using var fsOut = File.Create(filename);
            await zippedFileStream.CopyToAsync(fsOut);
            zippedFileStream.Position = 0;
            Console.WriteLine($"Dump archive saved to: {filename}");
        }

        Console.WriteLine($"Archive size: {zippedFileStream.Length / 1024} kb");

        _uploading = true;
        await azureService.UploadToStorage(zippedFileStream, filename);

        stopWatch.Stop();
        var ts = stopWatch.Elapsed;
        var elapsedTime = $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        Console.WriteLine($"Database backup is complete\nTook {elapsedTime}");
        return 0;
    }

    private static void UploadProgressCallback(long current, long length)
    {
        if (!_uploading) 
            return;
        
        // TODO: don't do too frequent updates...
        var percent = Convert.ToDouble(current) / length * 100;
        Console.WriteLine($"Upload: {percent:F0}%");
    }
}