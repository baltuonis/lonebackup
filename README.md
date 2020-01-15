# LoneBackup

Dotnet core
MySql
AzureStorage
Sentry?

## Usage

```
lonebackup -c config.local.json
```

Sample config.json

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

## TODO:

1. Backup rotation
