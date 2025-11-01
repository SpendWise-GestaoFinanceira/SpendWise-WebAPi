# SpendWise - Sistema de Gerenciamento Financeiro Pessoal

## Sobre o Projeto

Sistema enterprise completo para gerenciamento de financas pessoais, construido com Clean Architecture, CQRS e as melhores praticas de desenvolvimento.

### Transformacao
- **De:** Console app Java basico
- **Para:** Sistema enterprise completo (.NET 9 + Next.js + PostgreSQL)

## Estrutura do Projeto

```
MateusOrlando-TPPE-2025.2-WebAPi/
├── src/                    # Codigo fonte (.NET)
│   ├── SpendWise.Domain/
│   ├── SpendWise.Application/
│   ├── SpendWise.Infrastructure/
│   └── SpendWise.API/
├── tests/                  # Testes unitarios
│   ├── SpendWise.Domain.Tests/
│   ├── SpendWise.Application.Tests/
│   └── SpendWise.Infrastructure.Tests/
├── infrastructure/         # Docker, DB, nginx
│   ├── Dockerfile
│   ├── Dockerfile.dev
│   ├── docker-compose.yml
│   └── database/
│       └── migrations.sql
├── docs/                   # Documentacao
│   └── examples/
│       └── transacoes_exemplo.csv
├── scripts/                # Scripts utilitarios
│   ├── dev.sh             # Linux/Mac
│   └── dev.ps1            # Windows
├── .github/                # CI/CD (futuro)
├── .env                    # Variaveis de ambiente (NAO COMMITAR)
├── .env.example            # Template de variaveis
├── SpendWise.sln          # Solucao .NET
└── README.md
```

## Quick Start

### Pre-requisitos
- Docker Desktop
- (Opcional) .NET 9 SDK para desenvolvimento local

### Iniciar Ambiente de Desenvolvimento

**Windows (PowerShell):**
```powershell
# 1. Criar arquivo .env (se nao existir)
Rename-Item ".env-COPIAR-MANUALMENTE.txt" ".env"

# 2. Iniciar ambiente
.\scripts\dev.ps1
```

**Linux/Mac (Bash):**
```bash
# 1. Criar arquivo .env (se nao existir)
mv .env-COPIAR-MANUALMENTE.txt .env

# 2. Dar permissao de execucao
chmod +x scripts/dev.sh

# 3. Iniciar ambiente
./scripts/dev.sh
```

### Acessar Servicos

- **API:** http://localhost:5000
- **Swagger:** http://localhost:5000/swagger
- **Health Check:** http://localhost:5000/health
- **Adminer:** http://localhost:8080

## Como Executar

### **Pré-requisitos:**
- Docker Desktop
- .NET 9 SDK
- Next.js


### **1. Infraestrutura:**
```bash
cd SpendWise/infraestrutura
cp .env.example .env
docker compose up -d
```

### **2. Acesso:**
- **Adminer:** http://localhost:8080
- **API:** http://localhost:5000 (CP-02)
- **Frontend:** http://localhost:3000 (CP-02)

### **3. Backend (CP-02):**
```bash
cd SpendWise/backend
dotnet run
```

### **4. Frontend (CP-02):**
```bash
cd SpendWise/frontend
npm install
npm run dev
```

## **Roadmap & Commits (3 Checkpoints)**

### **CP-01: INFRAESTRUTURA E MODELAGEM**
- Docker + PostgreSQL + Adminer
- Diagrama de Classes (Domain Model)
- MER/DER detalhado com todas as entidades
- DDL completo com regras de negócio
- Especificações das 6 regras de negócio
- Convenções de commit e documentação

### **CP-02: APLICAÇÃO COMPLETA**
- **Backend:** ASP.NET Core + Clean Architecture + CQRS
- **Frontend:** Next.js + React + Tailwind CSS
- **Integração:** API completa + UI funcional
- **Funcionalidades:** Todas as 6 regras implementadas

### **CP-03: QUALIDADE E DEPLOY**
- **Testes:** Unitários + Integração + E2E
- **CI/CD:** GitHub Actions + Deploy automatizado
- **Produção:** Aplicação deployada e funcional
- **Documentação:** Guias completos de uso

## **Desenvolvido por**

**Mateus Orlando** - TPPE 2025.2 - UnB

## **Licença**

Projeto acadêmico - Técnicas de Programação em Plataformas Emergentes
