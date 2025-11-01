#!/bin/bash

# ========================================
# SpendWise - Script de Desenvolvimento
# ========================================
# Script para iniciar ambiente de desenvolvimento local
# Servicos: PostgreSQL + API .NET + Adminer

set -e  # Parar em caso de erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Navegar para o diretorio raiz do projeto
cd "$(dirname "$0")/.."

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  SpendWise - Ambiente de Desenvolvimento${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# ========================================
# 1. Verificar Docker
# ========================================
echo -e "${YELLOW}[1/6]${NC} Verificando Docker..."
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker nao esta rodando.${NC}"
    echo -e "${YELLOW}Por favor, inicie o Docker Desktop e tente novamente.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker esta rodando${NC}"
echo ""

# ========================================
# 2. Verificar arquivo .env
# ========================================
echo -e "${YELLOW}[2/6]${NC} Verificando arquivo .env..."
if [ ! -f .env ]; then
    echo -e "${RED}❌ Arquivo .env nao encontrado.${NC}"
    echo -e "${YELLOW}Criando .env a partir do .env.example...${NC}"
    
    if [ -f .env.example ]; then
        cp .env.example .env
        echo -e "${GREEN}✓ Arquivo .env criado${NC}"
        echo -e "${YELLOW}⚠️  IMPORTANTE: Configure as variaveis no arquivo .env antes de continuar.${NC}"
        echo -e "${YELLOW}   Especialmente: JWT_SECRET_KEY e EMAIL_API_KEY${NC}"
        exit 1
    else
        echo -e "${RED}❌ Arquivo .env.example nao encontrado.${NC}"
        exit 1
    fi
fi
echo -e "${GREEN}✓ Arquivo .env encontrado${NC}"
echo ""

# ========================================
# 3. Parar containers existentes
# ========================================
echo -e "${YELLOW}[3/6]${NC} Parando containers existentes..."
cd infrastructure
docker-compose down > /dev/null 2>&1 || true
cd ..
echo -e "${GREEN}✓ Containers parados${NC}"
echo ""

# ========================================
# 4. Perguntar sobre volumes
# ========================================
echo -e "${YELLOW}[4/6]${NC} Gerenciamento de volumes..."
read -p "Deseja remover volumes antigos do banco de dados? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Removendo volumes...${NC}"
    cd infrastructure
    docker-compose down -v
    cd ..
    echo -e "${GREEN}✓ Volumes removidos${NC}"
else
    echo -e "${BLUE}Mantendo volumes existentes${NC}"
fi
echo ""

# ========================================
# 5. Iniciar containers
# ========================================
echo -e "${YELLOW}[5/6]${NC} Iniciando containers..."
echo -e "${BLUE}Construindo e iniciando servicos...${NC}"
cd infrastructure
docker-compose up --build -d
cd ..

# Aguardar servicos ficarem prontos
echo -e "${YELLOW}Aguardando servicos ficarem prontos...${NC}"
sleep 10

echo -e "${GREEN}✓ Containers iniciados${NC}"
echo ""

# ========================================
# 6. Verificar status
# ========================================
echo -e "${YELLOW}[6/6]${NC} Verificando status dos containers..."
cd infrastructure
docker-compose ps
cd ..
echo ""

# ========================================
# Informacoes de acesso
# ========================================
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  Ambiente iniciado com sucesso!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${BLUE}Servicos disponiveis:${NC}"
echo -e "  ${GREEN}•${NC} API Backend:    ${YELLOW}http://localhost:5000${NC}"
echo -e "  ${GREEN}•${NC} Swagger:        ${YELLOW}http://localhost:5000/swagger${NC}"
echo -e "  ${GREEN}•${NC} Health Check:   ${YELLOW}http://localhost:5000/health${NC}"
echo -e "  ${GREEN}•${NC} PostgreSQL:     ${YELLOW}localhost:5432${NC}"
echo -e "  ${GREEN}•${NC} Adminer:        ${YELLOW}http://localhost:8080${NC}"
echo ""
echo -e "${BLUE}Credenciais do banco (Adminer):${NC}"
echo -e "  ${GREEN}•${NC} Sistema:   ${YELLOW}PostgreSQL${NC}"
echo -e "  ${GREEN}•${NC} Servidor:  ${YELLOW}postgres${NC}"
echo -e "  ${GREEN}•${NC} Usuario:   ${YELLOW}spendwise${NC}"
echo -e "  ${GREEN}•${NC} Senha:     ${YELLOW}spendwise123${NC}"
echo -e "  ${GREEN}•${NC} Database:  ${YELLOW}spendwise${NC}"
echo ""
echo -e "${BLUE}Comandos uteis:${NC}"
echo -e "  ${GREEN}•${NC} Ver logs:           ${YELLOW}cd infrastructure && docker-compose logs -f${NC}"
echo -e "  ${GREEN}•${NC} Ver logs da API:    ${YELLOW}cd infrastructure && docker-compose logs -f api${NC}"
echo -e "  ${GREEN}•${NC} Parar containers:   ${YELLOW}cd infrastructure && docker-compose down${NC}"
echo -e "  ${GREEN}•${NC} Reiniciar API:      ${YELLOW}cd infrastructure && docker-compose restart api${NC}"
echo ""
echo -e "${YELLOW}Pressione Ctrl+C para sair dos logs${NC}"
echo ""

# Seguir logs
cd infrastructure
docker-compose logs -f
