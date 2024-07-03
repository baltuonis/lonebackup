#!/bin/sh

cd ./LoneBackup.App && \
rm ./publish/* && \
dotnet publish -r linux-x64 --nologo -o ./publish && \
mv ./publish/LoneBackup.App ./publish/lonebackup-x64
