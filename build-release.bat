@echo off
echo ====================================
echo Building WindowsToolkit (RELEASE)
echo ====================================

dotnet publish WindowsToolkit.UI\WindowsToolkit.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true

if errorlevel 1 (
    echo.
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

echo.
echo Copying to Release\...
xcopy /y /q "WindowsToolkit.UI\bin\Release\net8.0-windows\win-x64\publish\*" "Release\"

echo.
echo ====================================
echo Build complete!
echo EXE location: Release\WindowsToolkit.UI.exe
echo ====================================

pause
