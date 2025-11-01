# 🚀 CI Pipeline - SpendWise Backend

## 📋 Visão Geral

Pipeline de Integração Contínua (CI) para o backend do SpendWise, garantindo qualidade de código, testes automatizados e build do Docker **sem deploy**.

## 🔄 Workflow

O pipeline é executado automaticamente em:
- **Push** para branches `main` ou `develop`
- **Pull Requests** para `main` ou `develop`

## 📊 Jobs do Pipeline

### 1. **Build** 🏗️
- **Objetivo:** Compilar a solução .NET
- **Ações:**
  - Checkout do código
  - Setup do .NET 9.0
  - Restore de dependências
  - Build em modo Release
  - Cache de artifacts para acelerar builds futuros

### 2. **Code Quality** 🔍
- **Objetivo:** Garantir qualidade e padrões de código
- **Ações:**
  - Build com warnings tratados como erros (`TreatWarningsAsErrors=true`)
  - Verificação de formatação com `dotnet format`
  - Análise estática de código

### 3. **Unit Tests** 🧪
- **Objetivo:** Executar testes unitários e gerar cobertura
- **Ações:**
  - Execução de todos os testes
  - Geração de relatório de cobertura (Cobertura XML)
  - Upload de resultados para Codecov
  - Artifacts de testes disponíveis para download

### 4. **Docker Build** 🐳 *(apenas em push para main)*
- **Objetivo:** Verificar se a imagem Docker pode ser construída
- **Ações:**
  - Build da imagem Docker
  - **NÃO faz push** para registry
  - Cache de layers para otimização

### 5. **CI Success** ✅
- **Objetivo:** Resumo do pipeline
- **Ações:**
  - Mensagem de sucesso consolidada
  - Confirmação de todos os checks

## 📁 Estrutura de Testes

```
tests/
├── SpendWise.API.Tests/          # Testes de Controllers e Integração
├── SpendWise.Application.Tests/  # Testes de Handlers e Commands
├── SpendWise.Domain.Tests/        # Testes de Entidades e Regras
└── SpendWise.Infrastructure.Tests/ # Testes de Repositórios
```

## 🎯 Cobertura de Testes

O pipeline gera relatórios de cobertura que incluem:
- **Coverage.cobertura.xml** - Formato Cobertura
- **test-results.trx** - Resultados dos testes

### Visualizar Cobertura Localmente

```bash
# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Gerar relatório HTML (requer ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html

# Abrir relatório
start ./coverage/report/index.html
```

## 🔧 Configuração Local

### Pré-requisitos
- .NET 9.0 SDK
- Docker (opcional, para build local)

### Executar Pipeline Localmente

```bash
# 1. Build
dotnet restore SpendWise.sln
dotnet build SpendWise.sln --configuration Release

# 2. Code Quality
dotnet build SpendWise.sln /p:TreatWarningsAsErrors=true
dotnet format SpendWise.sln --verify-no-changes

# 3. Tests
dotnet test SpendWise.sln --configuration Release --collect:"XPlat Code Coverage"

# 4. Docker Build
docker build -f infrastructure/Dockerfile -t spendwise-backend:local .
```

## 📈 Métricas e Badges

### Status do Pipeline
[![CI Pipeline](https://github.com/SEU-USUARIO/SEU-REPO/actions/workflows/ci.yml/badge.svg)](https://github.com/SEU-USUARIO/SEU-REPO/actions/workflows/ci.yml)

### Cobertura de Código
[![codecov](https://codecov.io/gh/SEU-USUARIO/SEU-REPO/branch/main/graph/badge.svg)](https://codecov.io/gh/SEU-USUARIO/SEU-REPO)

## 🚫 O Que NÃO Está Incluído

- ❌ Deploy automático
- ❌ Push de imagens Docker para registry
- ❌ Testes de integração com banco de dados externo
- ❌ Testes E2E

## 🔜 Próximos Passos

Para adicionar deploy no futuro:
1. Criar workflow separado `cd.yml`
2. Adicionar secrets do Docker Hub
3. Configurar ambiente de produção
4. Adicionar health checks pós-deploy

## 📝 Notas

- O pipeline é **rápido** graças ao cache de dependências
- Falhas em qualquer job **bloqueiam** o merge
- Artifacts de testes ficam disponíveis por **90 dias**
- Docker build só roda em push para `main` (economia de recursos)

## 🆘 Troubleshooting

### Build Falha
```bash
# Limpar cache local
dotnet clean
rm -rf **/bin **/obj
dotnet restore
```

### Testes Falham
```bash
# Executar testes com mais detalhes
dotnet test --logger "console;verbosity=detailed"
```

### Docker Build Falha
```bash
# Verificar Dockerfile
docker build -f infrastructure/Dockerfile -t test .

# Ver logs detalhados
docker build --progress=plain -f infrastructure/Dockerfile -t test .
```

---

**Última atualização:** 01/11/2025
**Versão do Pipeline:** 1.0.0
