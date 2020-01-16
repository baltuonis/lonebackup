# LoneBackup

Dotnet core
MySql
AzureStorage
Sentry?

## Sample usage

```bash
lonebackup -c config.local.json 
# (default: config.json)
```

Sample `config.json`

```json
{
  "AzureStorageConnectionString": "storagestring",
  "AzureStorageContainer": "container",
  "AzureStorageFolder": "folder",
  "ArchivePassword": "abcd",
  "MySQL": {
    "Host": "localhost",
    "Port": 3306,
    "User": "root",
    "Pwd": "toor",
    "Databases": "dbname"
  },
  "SentryDsn": "",
  "CreateLocalFile": false
}
```

Then create a cron entry

```bash
crontab -e
```

```crontab
5 1 * * * cd /home/devops/backup/ && ./lonebackup-x64
```

## Build

## TODO:

1. Backup rotation
