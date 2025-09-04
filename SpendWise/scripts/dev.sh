#!/bin/bash

# Script para desenvolvimento local
echo "🚀 Iniciando ambiente de desenvolvimento..."

# Verificar se Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando. Por favor, inicie o Docker Desktop."
    exit 1
fi

# Parar containers existentes
echo "🛑 Parando containers existentes..."
docker-compose -f docker-compose.dev.yml down

# Remover volumes antigos (opcional)
read -p "🗑️  Deseja remover volumes antigos? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "🗑️  Removendo volumes antigos..."
    docker volume prune -f
fi

# Construir e iniciar containers
echo "🔨 Construindo e iniciando containers..."
docker-compose -f docker-compose.dev.yml up --build -d

# Aguardar serviços ficarem prontos
echo "⏳ Aguardando serviços ficarem prontos..."
sleep 10

# Verificar status dos containers
echo "📊 Status dos containers:"
docker-compose -f docker-compose.dev.yml ps

# Verificar logs
echo "📋 Logs dos serviços:"
echo "Backend: http://localhost:5000"
echo "Frontend: http://localhost:3000"
echo "Adminer: http://localhost:8080"
echo "Swagger: http://localhost:5000/swagger"

echo "✅ Ambiente de desenvolvimento iniciado!"
echo "📝 Para ver logs: docker-compose -f docker-compose.dev.yml logs -f"
echo "🛑 Para parar: docker-compose -f docker-compose.dev.yml down"
