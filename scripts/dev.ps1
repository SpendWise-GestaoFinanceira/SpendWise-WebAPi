# SpendWise - Script de Desenvolvimento
# Script para iniciar ambiente de desenvolvimento local
# Servicos: PostgreSQL + API .NET + Adminer

$ErrorActionPreference = "Stop"

# Navegar para o diretorio raiz do projeto
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $scriptPath "..")

Write-Host "========================================" -ForegroundColor Blue
Write-Host "  SpendWise - Ambiente de Desenvolvimento" -ForegroundColor Blue
Write-Host "========================================" -ForegroundColor Blue
Write-Host ""

# Verificar Docker
Write-Host "[1/6] Verificando Docker..." -ForegroundColor Yellow
try {
    docker info | Out-Null
    Write-Host "OK - Docker esta rodando" -ForegroundColor Green
}
catch {
    Write-Host "ERRO - Docker nao esta rodando." -ForegroundColor Red
    Write-Host "Por favor, inicie o Docker Desktop e tente novamente." -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Verificar arquivo .env
Write-Host "[2/6] Verificando arquivo .env..." -ForegroundColor Yellow
if (-Not (Test-Path ".env")) {
    Write-Host "AVISO - Arquivo .env nao encontrado." -ForegroundColor Red
    Write-Host "Criando .env a partir do .env.example..." -ForegroundColor Yellow
    
    if (Test-Path ".env.example") {
        Copy-Item ".env.example" ".env"
        Write-Host "OK - Arquivo .env criado" -ForegroundColor Green
        Write-Host "IMPORTANTE: Configure as variaveis no arquivo .env antes de continuar." -ForegroundColor Yellow
        Write-Host "Especialmente: JWT_SECRET_KEY e EMAIL_API_KEY" -ForegroundColor Yellow
        exit 1
    }
    else {
        Write-Host "ERRO - Arquivo .env.example nao encontrado." -ForegroundColor Red
        exit 1
    }
}
Write-Host "OK - Arquivo .env encontrado" -ForegroundColor Green
Write-Host ""

# Parar containers existentes
Write-Host "[3/6] Parando containers existentes..." -ForegroundColor Yellow
try {
    Set-Location infrastructure
    docker-compose down 2>&1 | Out-Null
    Set-Location ..
}
catch {
    Set-Location ..
}
Write-Host "OK - Containers parados" -ForegroundColor Green
Write-Host ""

# Perguntar sobre volumes
Write-Host "[4/6] Gerenciamento de volumes..." -ForegroundColor Yellow
$response = Read-Host "Deseja remover volumes antigos do banco de dados? (y/N)"
if ($response -eq 'y' -or $response -eq 'Y') {
    Write-Host "Removendo volumes..." -ForegroundColor Yellow
    Set-Location infrastructure
    docker-compose down -v
    Set-Location ..
    Write-Host "OK - Volumes removidos" -ForegroundColor Green
}
else {
    Write-Host "Mantendo volumes existentes" -ForegroundColor Blue
}
Write-Host ""

# Iniciar containers
Write-Host "[5/6] Iniciando containers..." -ForegroundColor Yellow
Write-Host "Construindo e iniciando servicos..." -ForegroundColor Blue
Set-Location infrastructure
docker-compose up --build -d
Set-Location ..

Write-Host "Aguardando servicos ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "OK - Containers iniciados" -ForegroundColor Green
Write-Host ""

# Verificar status
Write-Host "[6/6] Verificando status dos containers..." -ForegroundColor Yellow
Set-Location infrastructure
docker-compose ps
Set-Location ..
Write-Host ""

# Informacoes de acesso
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Ambiente iniciado com sucesso!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Servicos disponiveis:" -ForegroundColor Blue
Write-Host "  - API Backend:    http://localhost:5000" -ForegroundColor Yellow
Write-Host "  - Swagger:        http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "  - Health Check:   http://localhost:5000/health" -ForegroundColor Yellow
Write-Host "  - PostgreSQL:     localhost:5432" -ForegroundColor Yellow
Write-Host "  - Adminer:        http://localhost:8080" -ForegroundColor Yellow
Write-Host ""
Write-Host "Credenciais do banco (Adminer):" -ForegroundColor Blue
Write-Host "  - Sistema:   PostgreSQL" -ForegroundColor Yellow
Write-Host "  - Servidor:  postgres" -ForegroundColor Yellow
Write-Host "  - Usuario:   spendwise" -ForegroundColor Yellow
Write-Host "  - Senha:     spendwise123" -ForegroundColor Yellow
Write-Host "  - Database:  spendwise" -ForegroundColor Yellow
Write-Host ""
Write-Host "Comandos uteis:" -ForegroundColor Blue
Write-Host "  - Ver logs:           cd infrastructure; docker-compose logs -f" -ForegroundColor Yellow
Write-Host "  - Ver logs da API:    cd infrastructure; docker-compose logs -f api" -ForegroundColor Yellow
Write-Host "  - Parar containers:   cd infrastructure; docker-compose down" -ForegroundColor Yellow
Write-Host "  - Reiniciar API:      cd infrastructure; docker-compose restart api" -ForegroundColor Yellow
Write-Host ""
Write-Host "Pressione Ctrl+C para sair dos logs" -ForegroundColor Yellow
Write-Host ""

# Seguir logs
Set-Location infrastructure
docker-compose logs -f
