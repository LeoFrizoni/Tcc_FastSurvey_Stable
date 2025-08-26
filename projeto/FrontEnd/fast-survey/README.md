# 🚀 FastSurvey Frontend

## 📋 Descrição

Frontend do sistema FastSurvey desenvolvido em React. Este é o projeto de TCC que oferece uma solução completa para criação, gerenciamento e análise de pesquisas interativas.

## 🛠️ Tecnologias Utilizadas

- **React 18** - Framework principal
- **React Router** - Roteamento
- **Axios** - Cliente HTTP
- **React Toastify** - Notificações
- **Chart.js** - Gráficos
- **CSS Modules** - Estilização
- **Lucide React** - Ícones

## 🚀 Instalação e Configuração

### Pré-requisitos

- Node.js 18+ 
- npm ou yarn
- Backend FastSurvey rodando (porta 5062)

### 1. Instalar Dependências

```bash
npm install
```

### 2. Configurar Variáveis de Ambiente

Copie o arquivo de exemplo e configure:

```bash
cp env.example .env
```

Edite o arquivo `.env`:

```env
# API Configuration
REACT_APP_API_URL=http://localhost:5062

# Google OAuth (opcional)
REACT_APP_GOOGLE_CLIENT_ID=seu-google-client-id-aqui

# Environment
REACT_APP_ENV=development
```

### 3. Configurar Google OAuth (Opcional)

Para usar o login com Google, siga as instruções em [GOOGLE_OAUTH_SETUP.md](./GOOGLE_OAUTH_SETUP.md).

### 4. Executar o Projeto

```bash
npm start
```

O projeto estará disponível em `http://localhost:3000`

## 📁 Estrutura do Projeto

```
src/
├── components/          # Componentes reutilizáveis
│   ├── layouts/        # Layouts e componentes de UI
│   ├── charts/         # Componentes de gráficos
│   └── utilities/      # Utilitários
├── pages/              # Páginas da aplicação
│   ├── admin/          # Páginas administrativas
│   ├── mobile/         # Versões mobile
│   └── ...             # Outras páginas
├── config/             # Configurações
├── helpers/            # Funções auxiliares
├── lib/                # Bibliotecas e APIs
└── routes/             # Configuração de rotas
```

## 🔧 Scripts Disponíveis

- `npm start` - Inicia o servidor de desenvolvimento
- `npm build` - Gera build de produção
- `npm test` - Executa testes
- `npm eject` - Ejecta configurações do Create React App

## 🚨 Solução de Problemas

### Erro de Google OAuth

Se você ver erros relacionados ao Google Sign-In:

1. Verifique se o arquivo `.env` está configurado
2. Confirme se o Google Client ID está correto
3. Verifique se `localhost:3000` está autorizado no Google Cloud Console
4. Consulte [GOOGLE_OAUTH_SETUP.md](./GOOGLE_OAUTH_SETUP.md)

### Erro de Conexão com API

Se não conseguir conectar com o backend:

1. Verifique se o backend está rodando na porta 5062
2. Confirme se `REACT_APP_API_URL` está correto no `.env`
3. Verifique se não há problemas de CORS
4. Execute o script de teste: `node test-api.js`

### Problemas de Login

Se o login não estiver funcionando:

1. Verifique se o backend está rodando
2. Confirme se os endpoints estão corretos
3. Verifique se há usuários no banco de dados
4. Consulte [LOGIN_SETUP.md](./LOGIN_SETUP.md) para configuração detalhada

### Erro de Compilação

Se houver erros de compilação:

1. Delete a pasta `node_modules` e `package-lock.json`
2. Execute `npm install` novamente
3. Verifique se está usando Node.js 18+

## 📱 Funcionalidades

### ✅ Implementadas

- ✅ **Sistema de Login** - Login tradicional e Google OAuth
- ✅ **Dashboard** - Visão geral das pesquisas
- ✅ **Criação de Pesquisas** - Interface drag & drop
- ✅ **Pesquisas Interativas** - Sistema estilo Kahoot
- ✅ **Analytics** - Gráficos e relatórios
- ✅ **Responsividade** - Versão mobile completa
- ✅ **Transições** - Animações suaves
- ✅ **Exportação** - PDF e Excel
- ✅ **QR Codes** - Geração automática

### 🔄 Em Desenvolvimento

- 🔄 **WebSockets** - Comunicação em tempo real
- 🔄 **PWA** - Progressive Web App
- 🔄 **Offline Mode** - Funcionalidade offline

## 🎨 Design System

### Cores Principais

- **Primária**: `#7c3aed` (Roxo)
- **Secundária**: `#3b82f6` (Azul)
- **Sucesso**: `#10b981` (Verde)
- **Erro**: `#ef4444` (Vermelho)
- **Aviso**: `#f59e0b` (Amarelo)

### Componentes

- **Botões**: Padronizados com estados hover/focus
- **Modais**: Sistema de modais responsivo
- **Formulários**: Validação em tempo real
- **Gráficos**: Chart.js com tema personalizado

## 🔒 Segurança

- **Autenticação JWT** - Tokens seguros
- **Validação** - Validação client-side e server-side
- **CORS** - Configurado adequadamente
- **HTTPS** - Recomendado para produção

## 📊 Performance

- **Lazy Loading** - Carregamento sob demanda
- **Code Splitting** - Divisão automática de código
- **Caching** - Cache inteligente
- **Compressão** - Assets otimizados

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT.

## 📞 Suporte

Para suporte ou dúvidas:

1. Verifique a documentação
2. Consulte os arquivos de configuração
3. Abra uma issue no repositório

---

**FastSurvey** - Transformando a forma como você coleta e analisa dados! 📊✨
