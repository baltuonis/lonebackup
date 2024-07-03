using System;
using System.IO;
using MySqlConnector;

namespace LoneBackup.App.Services
{
    public class MySqlService
    {
        private readonly AppConfig _appConfig;

        public MySqlService(AppConfig appConfig)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public Stream GetDatabaseBackup(string dbName)
        {
            var connString =
                $"server={_appConfig.DbHost};user={_appConfig.DbUser};pwd={_appConfig.DbPwd};database={dbName};port={_appConfig.DbPort};";

            // Important Additional Connection Options
            connString += "charset=utf8;convertzerodatetime=true;";

            using var conn = new MySqlConnection(connString);
            using var cmd = new MySqlCommand();
            using var mySqlBackup = new MySqlBackup(cmd);
            var backupStream = new MemoryStream();

            Console.WriteLine($"DB: Connecting to {conn.DataSource}/{conn.Database}");

            cmd.Connection = conn;
            conn.Open();
            Console.WriteLine($"DB: Dumping `{conn.Database}`...");

            mySqlBackup.ExportToMemoryStream(backupStream);
            backupStream.Seek(0, SeekOrigin.Begin);
            conn.Close();
            
            return backupStream;
        }
    }
}