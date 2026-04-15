@echo off
cd ../../

call git submodule update --init --recursive
call dotnet build Content.Packaging --configuration Release
call dotnet run --project Content.Packaging server --platform win-x64 --platform linux-x64
call dotnet run --project Content.Packaging client --no-wipe-release

pause
