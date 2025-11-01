# Multi-stage build para otimizar tamanho da imagem
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

# Copiar arquivos de solução e projetos
COPY ["Directory.Build.props", "./"]
COPY ["SpendWise.sln", "./"]
COPY ["src/SpendWise.API/SpendWise.API.csproj", "src/SpendWise.API/"]
COPY ["src/SpendWise.Application/SpendWise.Application.csproj", "src/SpendWise.Application/"]
COPY ["src/SpendWise.Domain/SpendWise.Domain.csproj", "src/SpendWise.Domain/"]
COPY ["src/SpendWise.Infrastructure/SpendWise.Infrastructure.csproj", "src/SpendWise.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "SpendWise.sln"

# Copiar todo o código
COPY ["src/", "src/"]

# Build da aplicação
WORKDIR "/build"
RUN dotnet build "SpendWise.sln" -c Release --no-restore

# Publish da aplicação
FROM build AS publish
RUN dotnet publish "src/SpendWise.API/SpendWise.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copiar aplicação publicada
COPY --from=publish /app/publish .

# Expor porta
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicialização
ENTRYPOINT ["dotnet", "SpendWise.API.dll"]