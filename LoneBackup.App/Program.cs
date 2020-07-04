using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using LoneBackup.App.Services;
using McMaster.Extensions.CommandLineUtils;
using ConfigurationBuilder = LoneBackup.App.Config.ConfigurationBuilder;

namespace LoneBackup.App
{
    // TODO: add sentry
    [Command(Name = "lonebackup",
        Description =
            "Simple, zero-dependencies, standalone backup utility for uploading MySQL/MariaDB dumps to Azure Blob Storage.")]
    [HelpOption("-?")]
    public class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option(Description = "Configuration file path", ShortName = "c", ShowInHelpText = true)]
        public string ConfigFile { get; } = "config.json";

        private const string APP_VERSION = "0.0.6";

        private AppConfig _config;
        private bool _uploading = false;

        public async Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("LoneBackup v" + APP_VERSION);

            if (string.IsNullOrEmpty(ConfigFile))
            {
                app.ShowHelp();
                return 0;
            }

            var configuration = new ConfigurationBuilder(ConfigFile);
            _config = configuration.Build();

            var mysqlService = new MySqlService(_config);
            var azureService = new AzureStorageService(_config);

            Console.WriteLine("Compressing...");

            using var zippedFileStream = new ObservableMemoryStream(UploadProgressCallback);
            // using var zippedFileStream = new MemoryStream();
            using var zipStream = new ZipOutputStream(zippedFileStream);

            zipStream.SetLevel(9);
            zipStream.Password = _config.ArchivePassword;

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
                    Console.WriteLine($"Failed access database `{dbName}`. Exception:");
                    Console.WriteLine(ex.Message);
                    // TODO: log sentry
                    continue;
                }

                if (backupStream == null) continue;

                var entry = new ZipEntry(ZipEntry.CleanName(dbName + ".sql"))
                {
                    DateTime = DateTime.Now,
                    Size = backupStream.Length,
                };
                zipStream.PutNextEntry(entry);
                StreamUtils.Copy(backupStream, zipStream, new byte[4096]);
                zipStream.CloseEntry();
                Console.WriteLine($"DB backup `{dbName}` added.");
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

            // TODO: remove old archives (rotation)

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine($"Backup complete \nTook {elapsedTime}");
            return 0;
        }

        private void UploadProgressCallback(long current, long length)
        {
            if (!_uploading) return;
            var percent = Convert.ToDouble(current) / length * 100;
            Console.WriteLine($"Upload: {percent:F0}%");
        }
    }
}
