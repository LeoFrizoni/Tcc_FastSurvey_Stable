# Configuração do Google Sign-In

## Como obter o Google Client ID

1. **Acesse o Google Cloud Console**
   - Vá para https://console.cloud.google.com/
   - Faça login com sua conta Google

2. **Crie um projeto ou selecione um existente**
   - Clique no seletor de projetos no topo
   - Clique em "Novo Projeto" ou selecione um projeto existente

3. **Ative a API do Google Identity**
   - No menu lateral, vá para "APIs & Services" > "Library"
   - Procure por "Google Identity Services API"
   - Clique e ative a API

4. **Configure as credenciais**
   - Vá para "APIs & Services" > "Credentials"
   - Clique em "Create Credentials" > "OAuth 2.0 Client IDs"
   - Selecione "Web application"

5. **Configure as origens autorizadas**
   - **Authorized JavaScript origins:**
     - `http://localhost:3000` (para desenvolvimento)
     - `http://localhost:5173` (para Vite)
     - Seu domínio de produção (ex: `https://seusite.com`)
   
   - **Authorized redirect URIs:**
     - `http://localhost:3000`
     - `http://localhost:5173`
     - Seu domínio de produção

6. **Copie o Client ID**
   - Após criar, você receberá um Client ID
   - Copie este ID

## Configuração no projeto

### Opção 1: Arquivo .env (Recomendado)

Crie um arquivo `.env` na raiz do projeto (`projeto/FrontEnd/fast-survey/.env`):

```env
# Configurações da API
VITE_API_URL=http://localhost:5062

# Google OAuth Client ID
VITE_GOOGLE_CLIENT_ID=seu-google-client-id-aqui

# Configurações de ambiente
VITE_ENV=development
```

### Opção 2: Variável global no HTML

Adicione no arquivo `public/index.html`:

```html
<script>
  window.GOOGLE_CLIENT_ID = 'seu-google-client-id-aqui';
</script>
```

## Testando

1. Reinicie o servidor de desenvolvimento
2. O botão do Google deve aparecer na tela de login
3. Se não aparecer, verifique:
   - Se o Client ID está correto
   - Se as origens autorizadas incluem `http://localhost:3000` ou `http://localhost:5173`
   - Se a API do Google Identity está ativada

## Solução de problemas

- **Botão não aparece**: Verifique se o `VITE_GOOGLE_CLIENT_ID` está definido
- **Erro de origem não autorizada**: Adicione `http://localhost:3000` nas origens autorizadas
- **Erro de API**: Certifique-se de que a Google Identity Services API está ativada
