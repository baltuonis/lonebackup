# LoneBackup

A simple single executable tool to back up your MySQL/MariaDB databases to Azure Blob Storage.

Purpose: no docker requirement, deploy anywhere, simple configuration

## Quick start

SSH to your server

```bash
mkdir -p /var/lonebackup && cd "$_"

# Get the latest release
wget https://github.com/baltuonis/lonebackup/releases/download/v1.0.3/lonebackup-x64
# Get sample config
wget https://github.com/baltuonis/lonebackup/blob/master/LoneBackup.App/config.json

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

1. Copy `lonebackup-x64` to remote machine
2. Add `config.json`
3. Copy `Dockerfile-remote` as `Dockerfile` to remote machine
4. Run commands (remote):

```bash
docker build -t lonebackup . 
docker run --rm --network host lonebackup 
```

## TODO:

1. Backup rotation (delete after X days)
2. Allow configuring storage tier https://learn.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview
2. Backup folders
3. App Insights logging
4. Upgrade Azure Storage packages
5. Create a GitHub action for releases
6. Pull credentials from Azure AppConfig/KeyVault
