#!/bin/sh

cd ./LoneBackup.App && \
#rm ./publish/* && \
dotnet publish -r linux-x64 --nologo -o ./publish --self-contained -c Release -p:IncludeNativeLibrariesForSelfExtract=true -p:NativeDebugSymbols=false && \
mv ./publish/LoneBackup.App ./publish/lonebackup-x64
