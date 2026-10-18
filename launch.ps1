Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  FeteCam - Lancement de l'application" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Set-Location $PSScriptRoot

Write-Host "Verification de dotnet..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERREUR] .NET SDK n'est pas installe." -ForegroundColor Red
    Write-Host "Veuillez installer .NET 10 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    Read-Host "Appuyez sur Entree pour quitter"
    exit 1
}

Write-Host ""
Write-Host "Restauration des packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERREUR] La restauration a echoue." -ForegroundColor Red
    Read-Host "Appuyez sur Entree pour quitter"
    exit 1
}

Write-Host ""
Write-Host "Compilation et lancement de FeteCam..." -ForegroundColor Green
Write-Host ""
dotnet run --project FeteCam.csproj -f net10.0-windows10.0.19041.0

Read-Host "Appuyez sur Entree pour quitter"
