using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using LoneBackup.App.Services;
using LoneBackup.App.Utils;
using McMaster.Extensions.CommandLineUtils;
using ConfigurationBuilder = LoneBackup.App.Config.ConfigurationBuilder;

namespace LoneBackup.App;

[Command(Name = "lonebackup",
    Description =
        "Simple, zero-dependencies, standalone backup utility for uploading MySQL/MariaDB dumps to Azure Blob Storage.")]
[HelpOption("-?")]
public class Program
{
    static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    [Option(Description = "Configuration file path", ShortName = "c", ShowInHelpText = true)]
    public string ConfigFile { get; set; } = "config.json";

    private const string AppVersion = "1.0.0";

    private AppConfig _config;
    private bool _uploading;

    public async Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        var stopWatch = Stopwatch.StartNew();
        Console.WriteLine("LoneBackup v" + AppVersion);

        if (string.IsNullOrEmpty(ConfigFile))
        {
            app.ShowHelp();
            return 0;
        }

        var configuration = new ConfigurationBuilder(ConfigFile);
        _config = configuration.Build();

        var mysqlService = new MySqlService(_config);
        var azureService = new AzureStorageService(_config);

        Console.WriteLine("Starting database(s) dump process");

        using var zippedFileStream = new ObservableMemoryStream(UploadProgressCallback);
        using var zipStream = new ZipOutputStream(zippedFileStream);

        zipStream.SetLevel(9);
        zipStream.Password = _config.ArchivePassword;

        var successfulDumpsCount = 0;
        // Add zip entries for each database
        foreach (var dbName in _config.Databases)
        {
            Stream backupStream;
            try
            {
                backupStream = mysqlService.GetDatabaseBackup(dbName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB: Failed access database `{dbName}`. Exception:");
                Console.WriteLine(ex);
                continue;
            }

            successfulDumpsCount++;
                
            var entry = new ZipEntry(ZipEntry.CleanName(dbName + ".sql"))
            {
                DateTime = DateTime.Now,
                Size = backupStream.Length,
            };
            zipStream.PutNextEntry(entry);
            StreamUtils.Copy(backupStream, zipStream, new byte[4096]);
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

        if (_config.CreateLocalFile)
        {
            using (var fsOut = File.Create(filename))
            {
                zippedFileStream.CopyTo(fsOut);
                zippedFileStream.Position = 0;
            }
        }

        Console.WriteLine("Compression complete");
        Console.WriteLine($"Archive size: {zippedFileStream.Length / 1024} kb");

        _uploading = true;
        await azureService.UploadToStorage(zippedFileStream, filename);

        stopWatch.Stop();
        var ts = stopWatch.Elapsed;
        var elapsedTime = $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        Console.WriteLine($"Database backup is complete\nTook {elapsedTime}");
        return 0;
    }

    private void UploadProgressCallback(long current, long length)
    {
        if (!_uploading) return;
        var percent = Convert.ToDouble(current) / length * 100;
        Console.WriteLine($"Upload: {percent:F0}%");
    }
}