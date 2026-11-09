@echo off
REM Build ArtCraftPresence as a single .exe (no .NET install needed on the target PC)
cd /d "%~dp0.."
dotnet publish App\App.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none ^
  -o publish
echo.
echo Done: publish\ArtCraftPresence.exe
pause
