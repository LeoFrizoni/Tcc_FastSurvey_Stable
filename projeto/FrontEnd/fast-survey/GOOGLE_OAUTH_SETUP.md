# 🔐 Configuração do Google OAuth para FastSurvey

## 📋 Pré-requisitos

Para usar o login com Google no FastSurvey, você precisa configurar o Google OAuth 2.0. Siga estas instruções:

## 🚀 Passo a Passo

### 1. Criar Projeto no Google Cloud Console

1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione um existente
3. Ative a faturação (necessário para APIs)

### 2. Habilitar APIs Necessárias

1. Vá para "APIs e Serviços" > "Biblioteca"
2. Procure e habilite as seguintes APIs:
   - **Google+ API** (ou Google Identity API)
   - **Google OAuth2 API**

### 3. Criar Credenciais OAuth 2.0

1. Vá para "APIs e Serviços" > "Credenciais"
2. Clique em "Criar Credenciais" > "ID do cliente OAuth 2.0"
3. Configure o tipo de aplicativo:
   - **Tipo**: Aplicativo da Web
   - **Nome**: FastSurvey Web App

### 4. Configurar URLs Autorizadas

#### URLs de Redirecionamento Autorizadas:
```
http://localhost:3000
http://localhost:3001
http://localhost:5062
https://seu-dominio-producao.com
```

#### Origens JavaScript Autorizadas:
```
http://localhost:3000
http://localhost:3001
http://localhost:5062
https://seu-dominio-producao.com
```

### 5. Obter o Client ID

Após criar as credenciais, você receberá:
- **Client ID**: `123456789-abcdefghijklmnop.apps.googleusercontent.com`
- **Client Secret**: (mantenha seguro, não use no frontend)

### 6. Configurar no Projeto

1. Crie um arquivo `.env` na raiz do projeto frontend:
```bash
# .env
REACT_APP_GOOGLE_CLIENT_ID=seu-client-id-aqui
REACT_APP_API_URL=http://localhost:5062
```

2. Reinicie o servidor de desenvolvimento:
```bash
npm start
```

## 🔧 Configuração Alternativa

Se não quiser usar o Google OAuth, você pode:

1. **Desabilitar o botão**: Comente o componente `GoogleLoginButton` no arquivo de login
2. **Usar apenas login local**: Configure apenas o formulário de login tradicional

## 🚨 Solução de Problemas

### Erro: "The given origin is not allowed for the given client ID"

**Causa**: O domínio não está autorizado no Google Cloud Console

**Solução**:
1. Verifique se `localhost:3000` está nas "Origens JavaScript Autorizadas"
2. Certifique-se de que o Client ID está correto
3. Aguarde alguns minutos após a configuração (pode levar tempo para propagar)

### Erro: "Failed to load resource: accounts.google.com"

**Causa**: Client ID inválido ou não configurado

**Solução**:
1. Verifique se o arquivo `.env` existe
2. Confirme se `REACT_APP_GOOGLE_CLIENT_ID` está configurado
3. Reinicie o servidor após alterações no `.env`

### Erro: "Google script not loaded"

**Causa**: Problema de conectividade ou bloqueio de rede

**Solução**:
1. Verifique a conexão com a internet
2. Desabilite bloqueadores de anúncios temporariamente
3. Verifique se o firewall não está bloqueando `accounts.google.com`

## 📝 Exemplo de Configuração Completa

### Arquivo `.env`:
```env
# Google OAuth
REACT_APP_GOOGLE_CLIENT_ID=123456789-abcdefghijklmnop.apps.googleusercontent.com

# API Configuration
REACT_APP_API_URL=http://localhost:5062

# Environment
REACT_APP_ENV=development
```

### Google Cloud Console - URLs Autorizadas:
```
http://localhost:3000
http://localhost:3001
http://localhost:5062
```

## 🔒 Segurança

- **Nunca** compartilhe o Client Secret
- **Sempre** use HTTPS em produção
- **Configure** domínios específicos (não use `*`)
- **Monitore** o uso das APIs no Google Cloud Console

## 📞 Suporte

Se ainda tiver problemas:

1. Verifique os logs do console do navegador
2. Confirme se todas as APIs estão habilitadas
3. Aguarde alguns minutos após alterações
4. Teste em uma janela anônima do navegador

---

**Nota**: Esta configuração é necessária apenas se você quiser usar o login com Google. O FastSurvey funciona perfeitamente sem esta funcionalidade.
