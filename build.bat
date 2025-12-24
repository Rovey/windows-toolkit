@echo off
echo ====================================
echo Building WindowsToolkit
echo ====================================

if "%1"=="release" (
    echo Building RELEASE version...
    dotnet build -c Release
    echo.
    echo ====================================
    echo Build complete!
    echo EXE location: WindowsToolkit.UI\bin\Release\net8.0-windows\WindowsToolkit.UI.exe
    echo ====================================
) else (
    echo Building DEBUG version...
    dotnet build
    echo.
    echo ====================================
    echo Build complete!
    echo EXE location: WindowsToolkit.UI\bin\Debug\net8.0-windows\WindowsToolkit.UI.exe
    echo ====================================
)

pause
