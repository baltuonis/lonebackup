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
        public string DbPwd { get; }
        public string[] Databases { get; }

        public AppConfig(string azureConnectionString, string azureContainer, string azureFolder, string archivePassword,
            string dbHost, string dbUser, string dbPwd, string[] databases)
        {
            AzureConnectionString = azureConnectionString;
            AzureContainer = azureContainer;
            AzureFolder = azureFolder;
            ArchivePassword = archivePassword;
            DbHost = dbHost;
            DbUser = dbUser;
            DbPwd = dbPwd;
            Databases = databases;
        }
    }
}