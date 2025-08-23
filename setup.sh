#!/bin/bash

echo "========================================"
echo "    FastSurvey - Setup do Projeto"
echo "========================================"
echo

echo "[1/4] Verificando pre-requisitos..."
echo

# Verificar se o .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "ERRO: .NET 8.0 SDK não encontrado!"
    echo "Por favor, instale o .NET 8.0 SDK: https://dotnet.microsoft.com/download"
    exit 1
fi
echo "✓ .NET SDK encontrado"

# Verificar se o Node.js está instalado
if ! command -v node &> /dev/null; then
    echo "ERRO: Node.js não encontrado!"
    echo "Por favor, instale o Node.js: https://nodejs.org/"
    exit 1
fi
echo "✓ Node.js encontrado"

# Verificar se o npm está instalado
if ! command -v npm &> /dev/null; then
    echo "ERRO: npm não encontrado!"
    exit 1
fi
echo "✓ npm encontrado"

echo
echo "[2/4] Restaurando dependencias do Backend..."
cd projeto/FastSurvey
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERRO: Falha ao restaurar dependencias do backend"
    exit 1
fi
echo "✓ Dependencias do backend restauradas"

echo
echo "[3/4] Instalando dependencias do Frontend..."
cd ../FrontEnd/fast-survey
npm install
if [ $? -ne 0 ]; then
    echo "ERRO: Falha ao instalar dependencias do frontend"
    exit 1
fi
echo "✓ Dependencias do frontend instaladas"

echo
echo "[4/4] Configuracoes finais..."
echo
echo "IMPORTANTE: Configure os seguintes arquivos antes de executar:"
echo
echo "1. Backend (projeto/FastSurvey/appsettings.json):"
echo "   - ConnectionString do PostgreSQL"
echo "   - Chave JWT"
echo "   - Configuracoes de email"
echo
echo "2. Frontend (projeto/FrontEnd/fast-survey/.env):"
echo "   - REACT_APP_API_URL"
echo "   - REACT_APP_GOOGLE_CLIENT_ID (opcional)"
echo
echo "3. Banco de dados:"
echo "   - Crie o banco 'fastsurvey' no PostgreSQL"
echo "   - Execute: dotnet ef database update"
echo
echo "========================================"
echo "    Setup concluido com sucesso!"
echo "========================================"
echo
echo "Para executar o projeto:"
echo
echo "Backend:"
echo "  cd projeto/FastSurvey"
echo "  dotnet run"
echo
echo "Frontend:"
echo "  cd projeto/FrontEnd/fast-survey"
echo "  npm start"
echo
