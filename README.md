# LoneBackup

A simple single executable tool to back up your MySQL/MariaDB databases to Azure Blob Storage.

Purpose: no docker requirement, deploy anywhere, simple configuration

## Quick start

SSH to your server

```bash
mkdir -p /var/lonebackup && cd "$_"

# Get the latest release
wget https://github.com/baltuonis/lonebackup/releases/download/v1.0.3/lonebackup-x64
chmod +x lonebackup-x64

# Get sample config
wget https://raw.githubusercontent.com/baltuonis/lonebackup/refs/heads/master/LoneBackup.App/config.json

# Edit config
vim config.json
```

Run backup manually (validate everything works)

```bash
./lonebackup-x64 
```

Then create a cron entry

```bash
crontab -e
```

```crontab
# https://crontab.guru/
5 1 * * * cd /var/lonebackup && ./lonebackup-x64
```

## Docker deployment

Use docker when having dependency problems on remote machines (old servers):

```bash
wget https://github.com/baltuonis/lonebackup/releases/download/v1.0.3/lonebackup-x64
chmod +x lonebackup-x64
wget https://raw.githubusercontent.com/baltuonis/lonebackup/refs/heads/master/LoneBackup.App/config.json
wget -O Dockerfile https://raw.githubusercontent.com/baltuonis/lonebackup/refs/heads/master/Dockerfile-remote

vim config.json

docker build -t lonebackup . 
# Test
docker run --rm --network host lonebackup:latest 
```


Cron config

```bash
crontab -e

0 8-23 * * * cd /var/lonebackup/ && docker run --rm --network host lonebackup 
```

### Docker upgrade

```bash
wget -O lonebackup-x64 https://github.com/baltuonis/lonebackup/releases/download/v1.0.3/lonebackup-x64
chmod +x lonebackup-x64

docker build -t lonebackup . 
```

## TODO:

1. Allow configuring storage tier https://learn.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview
2. Backup folders
3. App Insights logging
4. Upgrade Azure Storage packages
5. Create a GitHub action for releases
6. Pull credentials from Azure AppConfig/KeyVault
