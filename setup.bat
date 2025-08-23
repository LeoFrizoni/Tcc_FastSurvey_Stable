@echo off
echo ========================================
echo    FastSurvey - Setup do Projeto
echo ========================================
echo.

echo [1/4] Verificando pre-requisitos...
echo.

REM Verificar se o .NET está instalado
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: .NET 8.0 SDK nao encontrado!
    echo Por favor, instale o .NET 8.0 SDK: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo ✓ .NET SDK encontrado

REM Verificar se o Node.js está instalado
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: Node.js nao encontrado!
    echo Por favor, instale o Node.js: https://nodejs.org/
    pause
    exit /b 1
)
echo ✓ Node.js encontrado

REM Verificar se o npm está instalado
npm --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: npm nao encontrado!
    pause
    exit /b 1
)
echo ✓ npm encontrado

echo.
echo [2/4] Restaurando dependencias do Backend...
cd projeto\FastSurvey
dotnet restore
if %errorlevel% neq 0 (
    echo ERRO: Falha ao restaurar dependencias do backend
    pause
    exit /b 1
)
echo ✓ Dependencias do backend restauradas

echo.
echo [3/4] Instalando dependencias do Frontend...
cd ..\FrontEnd\fast-survey
npm install
if %errorlevel% neq 0 (
    echo ERRO: Falha ao instalar dependencias do frontend
    pause
    exit /b 1
)
echo ✓ Dependencias do frontend instaladas

echo.
echo [4/4] Configuracoes finais...
echo.
echo IMPORTANTE: Configure os seguintes arquivos antes de executar:
echo.
echo 1. Backend (projeto\FastSurvey\appsettings.json):
echo    - ConnectionString do PostgreSQL
echo    - Chave JWT
echo    - Configuracoes de email
echo.
echo 2. Frontend (projeto\FrontEnd\fast-survey\.env):
echo    - REACT_APP_API_URL
echo    - REACT_APP_GOOGLE_CLIENT_ID (opcional)
echo.
echo 3. Banco de dados:
echo    - Crie o banco 'fastsurvey' no PostgreSQL
echo    - Execute: dotnet ef database update
echo.
echo ========================================
echo    Setup concluido com sucesso!
echo ========================================
echo.
echo Para executar o projeto:
echo.
echo Backend:
echo   cd projeto\FastSurvey
echo   dotnet run
echo.
echo Frontend:
echo   cd projeto\FrontEnd\fast-survey
echo   npm start
echo.
pause
