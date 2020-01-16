using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace LoneBackup.App.Config
{
    public class ConfigurationBuilder
    {
        private readonly string _configFilename;
        private readonly IConfigurationRoot _configRoot;

        public ConfigurationBuilder(string configFilename)
        {
            _configFilename = configFilename;
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_configFilename);
            _configRoot = builder.Build();
        }

        public AppConfig Build()
        {

            var azureConnectionString = GetConfigString("AzureStorageConnectionString");
            var azureStorageContainer = GetConfigString("AzureStorageContainer");
            var azureStorageFolder = GetConfigString("AzureStorageFolder");
            var archivePassword = GetConfigString("ArchivePassword");
            var mySqlHost = GetConfigString("MySQL:Host");
            var mySqlUser = GetConfigString("MySQL:User");
            var mySqlPwd = GetConfigString("MySQL:Pwd");
            var mysqlDatabases = GetConfigStringArray("MySQL:Databases");
            var createLocalFile = Convert.ToBoolean(GetConfigString("CreateLocalFile"));

            if (mysqlDatabases.Length == 0)
            {
                throw new Exception($"Config error: please add at least one database inside `MySQL:Databases`");
                
            }

            var config = new AppConfig(azureConnectionString, azureStorageContainer, azureStorageFolder, archivePassword, mySqlHost,
                mySqlUser, mySqlPwd, mysqlDatabases, createLocalFile);
            
            return config;
        }

        private string[] GetConfigStringArray(string paramName)
        {
            var values = GetConfigString(paramName).Split(",");
            
            foreach (var item in values)
            {
                if (string.IsNullOrWhiteSpace(paramName))
                    throw new Exception($"Config error: argument `{nameof(paramName)}` has an empty member");
            }
            
            return values;
        }

        private string GetConfigString(string paramName)
        {
            var value = _configRoot[paramName];
            if (string.IsNullOrWhiteSpace(paramName))
                throw new Exception($"Config error: argument `{nameof(paramName)}` is empty");
            return value;
        }
    }
}