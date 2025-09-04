# Multi-stage build para otimizar tamanho da imagem
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["src/SpendWise.API/SpendWise.API.csproj", "src/SpendWise.API/"]
COPY ["src/SpendWise.Application/SpendWise.Application.csproj", "src/SpendWise.Application/"]
COPY ["src/SpendWise.Domain/SpendWise.Domain.csproj", "src/SpendWise.Domain/"]
COPY ["src/SpendWise.Infrastructure/SpendWise.Infrastructure.csproj", "src/SpendWise.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "src/SpendWise.API/SpendWise.API.csproj"

# Copiar todo o código
COPY . .

# Build da aplicação
WORKDIR "/src/src/SpendWise.API"
RUN dotnet build "SpendWise.API.csproj" -c Release -o /app/build

# Publish da aplicação
FROM build AS publish
RUN dotnet publish "SpendWise.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar dependências do sistema
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copiar aplicação publicada
COPY --from=publish /app/publish .

# Configurar usuário não-root
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expor porta
EXPOSE 5000
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Comando de inicialização
ENTRYPOINT ["dotnet", "SpendWise.API.dll"]