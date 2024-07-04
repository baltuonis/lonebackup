# LoneBackup

A simple single executable tool to back up your MySQL/MariaDB databases to Azure Blob Storage.

Purpose: no docker requirement, no fancy configuration files, low learning curve

## Quick start

SSH to your server

```bash
mkdir -p /var/lonebackup && cd "$_"
# Get the latest release
wget https://github.com/baltuonis/lonebackup/releases/download/v1.0.2/lonebackup-x64

vim config.json
```

*config.json*

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
  "CreateLocalFile": false
}
```

Run manual backup

```bash
./lonebackup-x64 
```

Then create a cron entry

```bash
crontab -e
```

```crontab
5 1 * * * cd /var/lonebackup && ./lonebackup-x64
```

## Docker deployment (for old machines)

Use docker when having dependency problems on remote machines (old servers):

1. Copy lonebackup to remote machine
2. Prepare config.json
3. Copy `Dockerfile-remote` to remote `Dockerfile`
4. Run commands:

```bash
docker build -t lonebackup . 
docker run --rm --network host lonebackup 
```

## TODO:

1. Backup rotation
2. App Insights logging
3. Upgrade Azure Storage packages
4. Create a GitHub action for releases
