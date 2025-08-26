# FastSurvey - Sistema de Pesquisas Interativas

Sistema completo para criação, gerenciamento e análise de pesquisas interativas, desenvolvido com ASP.NET Core (Backend) e React (Frontend).

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

### Frontend (React)
- **Interface responsiva** para desktop e mobile
- **Sistema de autenticação** com login/cadastro
- **Criador visual de pesquisas** com drag & drop
- **Visualização de resultados** com gráficos
- **Modo interativo** para apresentações
- **Upload de arquivos** com preview
- **Tema moderno** e intuitivo

## 🛠️ Tecnologias Utilizadas

### Backend
- **ASP.NET Core 8.0**
- **Entity Framework Core** com PostgreSQL
- **JWT Authentication**
- **AutoMapper** para mapeamento de objetos
- **FluentValidation** para validações
- **MailKit** para envio de emails
- **QRCoder** para geração de QR codes

### Frontend
- **React 18** com Hooks
- **React Router** para navegação
- **Axios** para requisições HTTP
- **React Toastify** para notificações
- **Lucide React** para ícones
- **CSS Modules** para estilização

### Banco de Dados
- **PostgreSQL** como banco principal

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
- Cadastro e login
- Recuperação de senha
- Perfis de usuário
- Tipos de usuário (Admin, Usuário)

### 2. Criação de Pesquisas
- Interface drag & drop
- Múltiplos tipos de pergunta
- Upload de anexos
- Organização em pastas

### 3. Pesquisas Interativas
- Modo apresentação
- Resultados em tempo real
- Códigos de acesso
- Sessões ativas

### 4. Análise de Resultados
- Gráficos interativos
- Exportação de dados
- Estatísticas detalhadas
- Dashboard de métricas

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

- **Seu Nome** - *Desenvolvimento inicial* - [SeuGitHub](https://github.com/seu-usuario)

## 🙏 Agradecimentos

- Professores orientadores
- Comunidade open source
- Contribuidores do projeto

## 📞 Suporte

Para suporte, envie um email para: suporte@fastsurvey.com

---

**FastSurvey** - Transformando a forma como você coleta e analisa dados! 📊✨
