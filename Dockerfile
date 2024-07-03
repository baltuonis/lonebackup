FROM mcr.microsoft.com/dotnet/runtime:8.0-jammy-chiseled

WORKDIR /app
COPY LoneBackup.App/publish/lonebackup-x64 .

ENTRYPOINT ["./lonebackup-x64"]