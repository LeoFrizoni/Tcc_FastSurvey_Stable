# FastSurvey

**FastSurvey** é uma aplicação desenvolvida em C# e React com o objetivo de facilitar a criação, distribuição e análise de pesquisas e questionários de forma rápida e eficiente. Este projeto foi desenvolvido como Trabalho de Conclusão de Curso (TCC) do curso de Sistemas de Informação do Centro Universitário Dom Bosco.

## 🎯 Objetivo

O objetivo principal do FastSurvey é oferecer uma plataforma simples, intuitiva e eficaz para a criação de formulários personalizados, coleta de respostas e geração de relatórios estatísticos, atendendo tanto usuários acadêmicos quanto empresariais.

## 📚 **DOCUMENTAÇÃO COMPLETA**

**Toda a documentação do projeto foi organizada na pasta `Documentacao_Projeto/`:**

- 📖 **[Documentação Completa](./Documentacao_Projeto/DOCUMENTACAO_COMPLETA_FASTSURVEY.md)** - Visão geral completa do sistema
- 📋 **[Índice da Documentação](./Documentacao_Projeto/INDICE_DOCUMENTACAO.md)** - Organização de todos os documentos
- 🔧 **[Correções Realizadas](./Documentacao_Projeto/CORREÇÕES_REALIZADAS.md)** - Histórico de correções
- 🚀 **[Melhorias Implementadas](./Documentacao_Projeto/MELHORIAS_IMPLEMENTADAS.md)** - Funcionalidades adicionadas
- 🔌 **[Novos Endpoints](./Documentacao_Projeto/NOVOS_ENDPOINTS.md)** - API disponível
- 🤖 **[Pipeline de Analise de Sentimento](./Documentacao_Projeto/PIPELINE_ANALISE_SENTIMENTO.md)** - Guia de configuração e validação do novo fluxo de IA

**Para uma visão completa do sistema, comece pela [Documentação Completa](./Documentacao_Projeto/DOCUMENTACAO_COMPLETA_FASTSURVEY.md).**

---

## 🚀 Funcionalidades

### Backend (ASP.NET Core)

- **Autenticação JWT** com refresh tokens
- **CRUD completo** de pesquisas, perguntas e respostas
- **Sistema de pastas** para organização
- **Upload de anexos** (imagens, PDFs)
- **Geração de QR Codes** para pesquisas
- **Pesquisas interativas** (tipo Kahoot)
- **Exportação de resultados** (PDF, Excel, CSV)
- **Sistema de permissões** por tipo de usuário
- **Envio de emails** para confirmação e reset de senha
- **Análise automática de sentimentos** para respostas discursivas, com persistência e dashboards

### Frontend (React)

- **Interface responsiva** para desktop e mobile
- **Sistema de autenticação** com login/cadastro
- **Criador visual de pesquisas** com drag & drop
- **Visualização de resultados** com gráficos
- **Modo interativo** para apresentações
- **Upload de arquivos** com preview
- **Tema moderno** e intuitivo

## 🛠️ Tecnologias Utilizadas

### Backend (ASP.NET Core 8.0)

- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8.0.2** - ORM para PostgreSQL
- **Npgsql.EntityFrameworkCore.PostgreSQL 8.0.2** - Provider PostgreSQL
- **Microsoft.AspNetCore.Authentication.JwtBearer 8.0** - Autenticação JWT
- **System.IdentityModel.Tokens.Jwt 8.14.0** - Geração e validação de tokens
- **Google.Apis.Auth 1.70.0** - Autenticação Google OAuth
- **Swashbuckle.AspNetCore 6.6.2** - Documentação Swagger/OpenAPI
- **Newtonsoft.Json 13.0.3** - Serialização JSON
- **QRCoder 1.4.3** - Geração de QR Codes
- **MailKit 4.13.0** - Cliente SMTP para envio de emails
- **MimeKit 4.13.0** - Manipulação de MIME types
- **itext7 8.0.2** - Geração de PDFs
- **EPPlus 7.0.0** - Manipulação de arquivos Excel
- **System.Drawing.Common 8.0.0** - Manipulação de imagens

### Frontend (React 19)

- **React 19.0.0** - Biblioteca principal com Hooks
- **React Router DOM 7.5.3** - Roteamento e navegação
- **Axios 1.9.0** - Cliente HTTP para requisições à API
- **React Toastify 11.0.5** - Sistema de notificações
- **Lucide React 0.508.0** - Biblioteca de ícones
- **React Icons 5.5.0** - Ícones adicionais
- **Chart.js 4.5.0** - Gráficos e visualizações
- **React Chart.js 2 5.3.0** - Wrapper React para Chart.js
- **QRCode React 4.2.0** - Geração de QR Codes no frontend
- **React Easy Crop 5.5.0** - Cropping de imagens
- **React Image File Resizer 0.4.8** - Redimensionamento de imagens
- **React Modal 3.16.3** - Sistema de modais
- **HTML2Canvas 1.4.1** - Captura de screenshots
- **jsPDF 3.0.1** - Geração de PDFs no frontend
- **@react-oauth/google 0.12.1** - Integração Google OAuth
- **CSS Modules** - Sistema de estilização modular

### Banco de Dados

- **PostgreSQL 12+** - Banco de dados principal
- **Entity Framework Core** - ORM e migrations

## 📋 Pré-requisitos

- **.NET 8.0 SDK**
- **Node.js 18+** e npm
- **PostgreSQL 12+**
- **Visual Studio 2022** ou **VS Code**

## 🔧 Instalação e Configuração

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/fastsurvey.git
cd fastsurvey
```

### 2. Configuração do Banco de Dados

#### Instalar PostgreSQL

- Baixe e instale o PostgreSQL: https://www.postgresql.org/download/
- Crie um banco de dados chamado `fastsurvey`

#### Configurar Connection String

Edite o arquivo `projeto/FastSurvey/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=fastsurvey;Username=seu_usuario;Password=sua_senha"
  }
}
```

#### Executar Migrations

```bash
cd projeto/FastSurvey
dotnet ef database update
```

### 3. Configuração do Backend

#### Instalar dependências

```bash
cd projeto/FastSurvey
dotnet restore
```

#### Configurar JWT e Email

Edite o arquivo `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "SuaChaveSecretaMuitoLongaAqui2024!@#$%^&*()",
    "Issuer": "FastSurvey",
    "Audience": "FastSurveyUsers"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-de-app",
    "EnableSsl": true
  }
}
```

#### Executar o Backend

```bash
dotnet run
```

O backend estará disponível em: `https://localhost:5062`

### 3.1. Configurar a IA de Sentimentos (novo pipeline)

1. **Habilite a seção `SentimentAI` no `appsettings.json`:**

```json
"SentimentAI": {
  "Enabled": true,
  "PythonPath": "python",
  "WorkingDirectory": "../sentiment-analysis-ai",
  "Script": "scripts/analyze_response.py",
  "TimeoutSeconds": 30
}
```

2. **Prepare o ambiente Python** dentro de `projeto/sentiment-analysis-ai`:

```powershell
cd projeto/sentiment-analysis-ai
python -m venv venv
.\venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

3. **Sincronize a conexão com o banco** (usa o mesmo PostgreSQL do FastSurvey). Ajuste se necessário:

```powershell
$env:SENTIMENT_DB_URL = "postgresql+psycopg2://USUARIO:SENHA@HOST:PORTA/BANCO"
```

4. **Teste o pipeline manualmente** (substitua os IDs reais):

```powershell
python .\scripts\analyze_response.py --resposta-id 123 --texto "Excelente atendimento, continuem assim"
```

A saída JSON é registrada no stdout e o resultado fica disponível na tabela `Analises`, aparecendo em `Resultados > Sentimentos` no frontend.

### 4. Configuração do Frontend

#### Instalar dependências

```bash
cd projeto/FrontEnd/fast-survey
npm install
```

#### Configurar variáveis de ambiente

Crie um arquivo `.env` na raiz do frontend:

```env
REACT_APP_API_URL=http://localhost:5062
REACT_APP_GOOGLE_CLIENT_ID=seu-google-client-id
```

#### Executar o Frontend

```bash
npm start
```

O frontend estará disponível em: `http://localhost:3000`

## 📁 Estrutura do Projeto

```
TCC_FastSurvey-develop/
├── Documentacao_Projeto/           # 📚 TODA A DOCUMENTAÇÃO
│   ├── DOCUMENTACAO_COMPLETA_FASTSURVEY.md
│   ├── INDICE_DOCUMENTACAO.md
│   ├── CORREÇÕES_REALIZADAS.md
│   └── ... (outros arquivos .md)
├── projeto/
│   ├── FastSurvey/                 # Backend ASP.NET Core
│   │   ├── Controllers/            # Controllers da API
│   │   ├── Dtos/                   # Data Transfer Objects
│   │   ├── Services/               # Lógica de negócio
│   │   ├── Infrastructure/         # Configurações e DI
│   │   └── Middleware/             # Middlewares customizados
│   ├── SISTEMA_FASTSURVEY.MODEL/   # Camada de dados
│   │   ├── Models/                 # Entidades do EF
│   │   ├── Repositories/           # Repositórios
│   │   └── Interfaces/             # Interfaces dos repositórios
│   └── FrontEnd/
│       └── fast-survey/            # Frontend React
│           ├── src/
│           │   ├── components/     # Componentes reutilizáveis
│           │   ├── pages/          # Páginas da aplicação
│           │   ├── lib/            # Utilitários e configurações
│           │   └── routes/         # Configuração de rotas
│           └── public/             # Arquivos estáticos
├── Diagramas/                      # Diagramas e documentação
└── Documentacao/                   # Documentação do TCC
```

## 🔐 Configuração de Autenticação

### JWT

O sistema usa JWT para autenticação. Configure as chaves no `appsettings.json`.

### Google OAuth (Opcional)

Para habilitar login com Google:

1. Crie um projeto no Google Cloud Console
2. Configure OAuth 2.0
3. Adicione o Client ID no arquivo `.env`

## 📊 Funcionalidades Principais

### 1. Sistema de Usuários

- Cadastro e login com autenticação JWT
- Login com Google OAuth
- Recuperação de senha via email
- Perfis de usuário com avatar
- Tipos de usuário (Admin, Usuário)

### 2. Criação de Pesquisas

- Interface drag & drop intuitiva
- Múltiplos tipos de pergunta (múltipla escolha, texto, nota, etc.)
- Upload de anexos (imagens, PDFs)
- Organização em pastas
- Geração de QR Codes para distribuição

### 3. Pesquisas Interativas

- Modo apresentação (tipo Kahoot)
- Resultados em tempo real
- Códigos de acesso para participantes
- Sessões ativas com controle de tempo

### 4. Análise de Resultados

- Gráficos interativos e estatísticos
- Exportação de dados (PDF, Excel, CSV)
- Estatísticas detalhadas
- Dashboard de métricas
- Análise de respostas por IA

### 5. Recursos Avançados

- Armazenamento seguro de respostas
- Interface amigável para administradores e respondentes
- Sistema de notificações
- Backup automático de dados

## 🚀 Deploy

### Backend (Azure/AWS)

```bash
dotnet publish -c Release
```

### Frontend (Vercel/Netlify)

```bash
npm run build
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 👥 Autores

- **Leonardo** - _Desenvolvimento Backend e Frontend_
- **Lucas** - _Desenvolvimento Backend e Frontend_
- **Marco** - _Desenvolvimento Backend e Frontend_
- **Rafael** - _Desenvolvimento Backend e Frontend_

**Trabalho de Conclusão de Curso (TCC)** - Curso de Sistemas de Informação do Centro Universitário Dom Bosco

## 🙏 Agradecimentos

- Professores orientadores
- Comunidade open source
- Contribuidores do projeto

## 📞 Suporte

Para suporte, envie um email para: suporte@fastsurvey.com

---

**FastSurvey** - Transformando a forma como você coleta e analisa dados! 📊✨

**📚 [Ver Documentação Completa](./Documentacao_Projeto/DOCUMENTACAO_COMPLETA_FASTSURVEY.md)**

---

## ⚠️ Disclaimer de Direitos Autorais

Este projeto é de autoria dos contribuintes do Repositório (Leonardo, Lucas, Marco e Rafael), desenvolvido como parte de um Trabalho de Conclusão de Curso.

**Todos os direitos reservados.** Este software não é de livre uso, redistribuição ou comercialização.

O código-fonte e demais arquivos disponibilizados aqui servem exclusivamente para fins educacionais e acadêmicos. A cópia, modificação ou uso do sistema para outros propósitos sem a autorização do autor é estritamente proibida.
