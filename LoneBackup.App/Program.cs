using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using LoneBackup.App.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

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

        [Option(Description = "Configuration file path")]
        public string ConfigFile { get; } = "config.json";

        private AppConfig _config;

        public async Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("LoneBackup v0.9.0");

            if (string.IsNullOrEmpty(ConfigFile))
            {
                app.ShowHelp();
                return 0;
            }

            _config = BuildConfiguration();

            var mysqlService = new MySqlService(_config);
            var azureService = new AzureStorageService(_config);


            Console.WriteLine("Compressing...");

            // using var fsOut = File.Create("zipfile.zip");
            // using (var localZipStream = new ZipOutputStream(fsOut))
            using var zippedFileStream = new ObservableMemoryStream(UploadProgressCallback);
            using var localZipStream = new ZipOutputStream(zippedFileStream);

            localZipStream.SetLevel(9);
            localZipStream.Password = _config.ArchivePassword;

            // Add zip entries for each database
            foreach (var dbName in _config.Databases)
            {
                var backupStream = mysqlService.GetDatabaseBackup(dbName);
                
                var entry = new ZipEntry(dbName + ".sql") {DateTime = DateTime.Now};
                localZipStream.PutNextEntry(entry);
                StreamUtils.Copy(backupStream, localZipStream, new byte[4096]);
                localZipStream.CloseEntry();
                localZipStream.IsStreamOwner = false;
                backupStream.Dispose();
                Console.WriteLine($"DB backup `{dbName}` added.");
            }

            // Zip file completed
            zippedFileStream.Position = 0;
            Console.WriteLine("Compression complete");
            Console.WriteLine($"Archive size: {zippedFileStream.Length / 1024} kb");

            await azureService.UploadToStorage(zippedFileStream);
            
            // TODO: remove old archives (rotation)

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine($"Backup complete \nTook {elapsedTime}");
            return 1;
        }

        private void UploadProgressCallback(long current, long length)
        {
            var percent = Convert.ToDouble(current) / length * 100;
            Console.WriteLine($"Upload: {percent:F0}%");
        }

        private AppConfig BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(ConfigFile);
            
            var cRoot = builder.Build();

            var azureConnectionString = cRoot["AzureStorageConnectionString"];
            var azureContainer = cRoot["AzureStorageContainer"];
            var azureFolder = cRoot["AzureStorageFolder"];
            var archivePassword = cRoot["ArchivePassword"];
            var dbHost = cRoot["MySQL:Host"];
            var dbUser = cRoot["MySQL:User"];
            var dbPwd = cRoot["MySQL:Pwd"];
            var databases = cRoot["MySQL:Databases"].Split(",");

            var config = new AppConfig(azureConnectionString, azureContainer, azureFolder, archivePassword, dbHost,
                dbUser,
                dbPwd, databases);

            // TODO: validate configuration
            // TODO: db count > 0
            // TODO: remove duplicates from dbs
            return config;
        }
    }
}