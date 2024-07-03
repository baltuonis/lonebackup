#!/bin/sh

cd ./LoneBackup.App && \
rm ./publish/* && \
dotnet publish -r linux-x64 --nologo -o ./publish --self-contained -c Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true && \
mv ./publish/LoneBackup.App ./publish/lonebackup-x64
