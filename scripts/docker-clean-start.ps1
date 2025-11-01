# Script para limpar e iniciar do zero
# Uso: .\scripts\docker-clean-start.ps1

Write-Host "Limpando ambiente Docker..." -ForegroundColor Blue

# Navegar para infrastructure
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $scriptPath "..\infrastructure")

# Parar tudo
Write-Host "Parando containers..." -ForegroundColor Yellow
docker-compose down -v 2>&1 | Out-Null

# Limpar volumes
Write-Host "Removendo volumes..." -ForegroundColor Yellow
docker volume prune -f 2>&1 | Out-Null

# Iniciar
Write-Host "Iniciando containers..." -ForegroundColor Yellow
docker-compose up --build -d

# Aguardar
Write-Host "Aguardando servicos (20 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

# Mostrar status
Write-Host ""
Write-Host "Status dos containers:" -ForegroundColor Cyan
docker-compose ps

# Verificar logs do postgres
Write-Host ""
Write-Host "Logs do PostgreSQL:" -ForegroundColor Cyan
docker-compose logs --tail=10 postgres

# Verificar logs da API
Write-Host ""
Write-Host "Logs da API:" -ForegroundColor Cyan
docker-compose logs --tail=10 api

Write-Host ""
Write-Host "Servicos disponiveis:" -ForegroundColor Green
Write-Host "  - API:     http://localhost:5000" -ForegroundColor Yellow
Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "  - Health:  http://localhost:5000/health" -ForegroundColor Yellow
Write-Host "  - Adminer: http://localhost:8080" -ForegroundColor Yellow
Write-Host ""
Write-Host "Para ver logs completos: docker-compose logs -f" -ForegroundColor Cyan
