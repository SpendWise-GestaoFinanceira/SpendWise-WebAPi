# Script simples para parar Docker
# Uso: .\scripts\docker-stop.ps1

Write-Host "Parando containers Docker..." -ForegroundColor Blue

# Navegar para infrastructure
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $scriptPath "..\infrastructure")

# Parar containers
docker-compose down

Write-Host "Containers parados com sucesso!" -ForegroundColor Green
