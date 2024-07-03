# LoneBackup

A simple single executable tool to back up your MySQL/MariaDB databases to Azure Blob Storage.

Purpose: no docker, no fancy configuration files, low learning curve

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
    "Databases": [
      "dbname1"
    ]
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
5 1 * * * cd /var/lonebackup && ./lonebackup-x64
```

## TODO:

1. Backup rotation
2. App Insights logging
3. Upgrade Azure Storage packages
4. Create a github action for releases
