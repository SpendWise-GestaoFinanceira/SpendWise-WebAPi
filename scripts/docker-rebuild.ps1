# Script para limpar e reconstruir containers
# Uso: .\scripts\docker-rebuild.ps1

Write-Host "Limpando e reconstruindo containers..." -ForegroundColor Blue

# Navegar para infrastructure
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $scriptPath "..\infrastructure")

# Parar e remover tudo
Write-Host "Parando containers..." -ForegroundColor Yellow
docker-compose down -v

# Limpar imagens antigas
Write-Host "Removendo imagens antigas..." -ForegroundColor Yellow
docker-compose down --rmi local

# Reconstruir
Write-Host "Reconstruindo containers..." -ForegroundColor Yellow
docker-compose up --build -d

# Aguardar
Write-Host "Aguardando servicos..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Mostrar status
docker-compose ps

# Mostrar logs
Write-Host ""
Write-Host "Ultimas linhas dos logs:" -ForegroundColor Cyan
docker-compose logs --tail=20

Write-Host ""
Write-Host "Servicos disponiveis:" -ForegroundColor Green
Write-Host "  - API:     http://localhost:5000" -ForegroundColor Yellow
Write-Host "  - Swagger: http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "  - Adminer: http://localhost:8080" -ForegroundColor Yellow
Write-Host ""
Write-Host "Para ver logs completos: cd infrastructure; docker-compose logs -f" -ForegroundColor Cyan
