using System;
using System.IO;
using System.Linq;
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
            // TODO: use option binding https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0
            var azureConnectionString = GetConfigString("AzureStorageConnectionString");
            var azureStorageContainer = GetConfigString("AzureStorageContainer");
            var azureStorageFolder = GetConfigString("AzureStorageFolder");
            var archivePassword = GetConfigString("ArchivePassword");
            var mySqlHost = GetConfigString("MySQL:Host");
            var mySqlPort = GetConfigString("MySQL:Port");
            var mySqlUser = GetConfigString("MySQL:User");
            var mySqlPwd = GetConfigString("MySQL:Pwd");
            var deleteOlderThanDays = GetConfigInt("Storage:DeleteOlderThanDays");
            var mysqlDatabases = GetConfigStringArray("MySQL:Databases");
            var createLocalFile = Convert.ToBoolean(GetConfigString("CreateLocalFile"));

            if (mysqlDatabases.Length == 0)
            {
                Console.WriteLine("No databases to backup");
            }

            var config = new AppConfig(azureConnectionString, azureStorageContainer, azureStorageFolder, archivePassword, 
                mySqlHost, mySqlPort, mySqlUser, mySqlPwd, mysqlDatabases, createLocalFile, deleteOlderThanDays);
            
            return config;
        }

        private string[] GetConfigStringArray(string section)
        {
            var values = _configRoot.GetSection(section)
                .GetChildren()
                .Select(c => c.Value?.Trim())
                .ToArray();
            
            foreach (var item in values)
            {
                if (string.IsNullOrWhiteSpace(item))
                    throw new Exception($"Config error: argument `{nameof(section)}` has an empty member");
            }
            
            return values;
        }
        
        private int GetConfigInt(string paramName)
        {
            var value = _configRoot[paramName];
            if (string.IsNullOrWhiteSpace(paramName))
                throw new Exception($"Config error: argument `{nameof(paramName)}` is empty");
            return Convert.ToInt32(value);
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