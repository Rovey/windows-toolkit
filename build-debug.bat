@echo off
echo ====================================
echo Building WindowsToolkit (DEBUG)
echo ====================================

dotnet publish WindowsToolkit.UI\WindowsToolkit.UI.csproj -c Debug -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true

if errorlevel 1 (
    echo.
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

echo.
echo ====================================
echo Build complete!
echo EXE location: WindowsToolkit.UI\bin\Debug\net8.0-windows\win-x64\publish\WindowsToolkit.UI.exe
echo ====================================

pause
