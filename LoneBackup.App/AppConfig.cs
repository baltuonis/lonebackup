using System;

namespace LoneBackup.App
{
    public class AppConfig
    {
        public string AzureConnectionString { get; }
        public string AzureContainer { get; }
        public string AzureFolder { get; }
        public string ArchivePassword { get; }
        public string DbHost { get; }
        public string DbUser { get; }
        public string DbPassword { get; }
        public int DbPort { get; }
        public string[] Databases { get; }
        public bool CreateLocalFile { get; }
        public int DeleteOlderThanDays { get; }

        public AppConfig(string azureConnectionString, string azureContainer, string azureFolder,
            string archivePassword,
            string dbHost, string dbPort, string dbUser, string dbPassword, string[] databases, bool createLocalFile,
            int deleteOlderThanDays)
        {
            AzureConnectionString = azureConnectionString;
            AzureContainer = azureContainer;
            AzureFolder = azureFolder;
            ArchivePassword = archivePassword;
            DbHost = dbHost;
            DbUser = dbUser;
            DbPassword = dbPassword;
            DbPort = Convert.ToInt32(dbPort);
            Databases = databases;
            CreateLocalFile = createLocalFile;
            DeleteOlderThanDays = deleteOlderThanDays;
        }
    }
}