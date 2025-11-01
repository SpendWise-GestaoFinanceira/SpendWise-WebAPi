# Script simples para iniciar Docker
# Uso: .\scripts\docker-start.ps1

Write-Host "Iniciando containers Docker..." -ForegroundColor Blue

# Navegar para infrastructure
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $scriptPath "..\infrastructure")

# Iniciar containers
docker-compose up --build -d

# Aguardar
Write-Host "Aguardando servicos..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Mostrar status
docker-compose ps

Write-Host ""
Write-Host "Servicos disponiveis:" -ForegroundColor Green
Write-Host "  - API:     http://localhost:5000" -ForegroundColor Yellow
Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "  - Adminer: http://localhost:8080" -ForegroundColor Yellow
Write-Host ""
Write-Host "Para ver logs: docker-compose logs -f" -ForegroundColor Cyan
Write-Host "Para parar:    docker-compose down" -ForegroundColor Cyan
