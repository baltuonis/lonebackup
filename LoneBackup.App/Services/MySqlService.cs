using System;
using System.IO;
using MySqlConnector;

namespace LoneBackup.App.Services;

public class MySqlService
{
    private readonly AppConfig _appConfig;

    public MySqlService(AppConfig appConfig)
    {
        _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
    }

    public void GetDatabaseBackup(string databaseName, Stream outputStream)
    {
        var connStrBldr = new MySqlConnectionStringBuilder
        {
            Server = _appConfig.DbHost,
            UserID = _appConfig.DbUser,
            Password = _appConfig.DbPassword,
            Database = databaseName,
            Port = (uint)_appConfig.DbPort,
            UseCompression = true,
            CharacterSet = "utf8",
            ConvertZeroDateTime = true
        };

        using var conn = new MySqlConnection(connStrBldr.ToString());
        using var cmd = new MySqlCommand();
        using var mySqlBackup = new MySqlBackup(cmd);
            
        Console.WriteLine($"DB: Connecting to {conn.DataSource}/{conn.Database}");

        cmd.Connection = conn;
        conn.Open();
        Console.WriteLine($"DB: Dumping `{conn.Database}`...");

        mySqlBackup.ExportToStream(outputStream);
        conn.Close();
    }
}