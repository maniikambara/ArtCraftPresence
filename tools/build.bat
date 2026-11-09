@echo off
REM Build ArtCraftPresence menjadi satu file .exe (tidak perlu install .NET di PC tujuan)
cd /d "%~dp0.."
dotnet publish App\App.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none ^
  -o publish
echo.
echo Selesai: publish\ArtCraftPresence.exe
pause
