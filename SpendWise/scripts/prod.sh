#!/bin/bash

# Script para produção
echo "🚀 Iniciando ambiente de produção..."

# Verificar se Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando. Por favor, inicie o Docker Desktop."
    exit 1
fi

# Verificar se arquivo .env existe
if [ ! -f .env ]; then
    echo "❌ Arquivo .env não encontrado. Criando template..."
    cat > .env << EOF
# Database
POSTGRES_DB=spendwise_prod
POSTGRES_USER=spendwise
POSTGRES_PASSWORD=your_secure_password_here

# JWT
JWT_SECRET_KEY=your_jwt_secret_key_here
JWT_ISSUER=SpendWise
JWT_AUDIENCE=SpendWise

# Frontend
NEXT_PUBLIC_API_URL=http://localhost:5000/api
EOF
    echo "⚠️  Por favor, configure as variáveis no arquivo .env antes de continuar."
    exit 1
fi

# Parar containers existentes
echo "🛑 Parando containers existentes..."
docker-compose -f docker-compose.prod.yml down

# Construir e iniciar containers
echo "🔨 Construindo e iniciando containers de produção..."
docker-compose -f docker-compose.prod.yml up --build -d

# Aguardar serviços ficarem prontos
echo "⏳ Aguardando serviços ficarem prontos..."
sleep 15

# Verificar status dos containers
echo "📊 Status dos containers:"
docker-compose -f docker-compose.prod.yml ps

# Verificar logs
echo "📋 Serviços disponíveis:"
echo "Aplicação: http://localhost"
echo "API: http://localhost/api"
echo "Swagger: http://localhost/swagger"

echo "✅ Ambiente de produção iniciado!"
echo "📝 Para ver logs: docker-compose -f docker-compose.prod.yml logs -f"
echo "🛑 Para parar: docker-compose -f docker-compose.prod.yml down"
