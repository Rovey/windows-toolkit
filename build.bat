@echo off
echo ====================================
echo Building WindowsToolkit
echo ====================================

if "%1"=="release" (
    echo Publishing RELEASE version as single-file exe...
    dotnet publish WindowsToolkit.UI\WindowsToolkit.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
    echo.
    echo ====================================
    echo Build complete!
    echo EXE location: WindowsToolkit.UI\bin\Release\net8.0-windows\win-x64\publish\WindowsToolkit.UI.exe
    echo ====================================
) else (
    echo Publishing DEBUG version as single-file exe...
    dotnet publish WindowsToolkit.UI\WindowsToolkit.UI.csproj -c Debug -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
    echo.
    echo ====================================
    echo Build complete!
    echo EXE location: WindowsToolkit.UI\bin\Debug\net8.0-windows\win-x64\publish\WindowsToolkit.UI.exe
    echo ====================================
)

pause
