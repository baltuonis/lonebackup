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

    public Stream GetDatabaseBackup(string databaseName)
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

        // TODO: can we avoid a temporary memory stream and put that directly to zip?
        var backupStream = new MemoryStream();
        mySqlBackup.ExportToMemoryStream(backupStream);
        backupStream.Seek(0, SeekOrigin.Begin);
        conn.Close();
            
        return backupStream;
    }
}