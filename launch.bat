@echo off
echo ========================================
echo   FeteCam - Lancement de l'application
echo ========================================
echo.

cd /d "%~dp0"

echo Verification de dotnet...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERREUR] .NET SDK n'est pas installe.
    echo Veuillez installer .NET 10 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo.
echo Restauration des packages...
dotnet restore
if errorlevel 1 (
    echo [ERREUR] La restauration a echoue.
    pause
    exit /b 1
)

echo.
echo Compilation et lancement de FeteCam...
echo.
dotnet run --project FeteCam.csproj -f net10.0-windows10.0.19041.0

pause
