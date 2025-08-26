# Configuração do Sistema de Login - FastSurvey

## Problemas Corrigidos

### 1. Endpoints Incorretos
- **Problema**: O frontend estava usando `/api/login/CadastrarLogin` mas o backend tem `/api/login/Cadastrar`
- **Solução**: Corrigido para usar o endpoint correto

### 2. Nomes de Propriedades Inconsistentes
- **Problema**: O backend retorna `loginId` mas o frontend estava verificando `Loginid`
- **Solução**: Padronizado para usar `loginId` e `tipoUsuarioId`

### 3. Validação de Senha Muito Restritiva
- **Problema**: A senha exigia 8 caracteres, maiúscula e número
- **Solução**: Simplificado para mínimo de 6 caracteres

### 4. Google Client ID Hardcoded
- **Problema**: Client ID estava hardcoded no código
- **Solução**: Removido e configurado via variável de ambiente

## Configuração Necessária

### 1. Variáveis de Ambiente
Copie o arquivo `env.example` para `.env` na raiz do projeto:

```bash
cp env.example .env
```

Configure as variáveis no arquivo `.env`:

```env
# URL da API backend
REACT_APP_API_URL=http://localhost:5062

# Google OAuth (opcional)
REACT_APP_GOOGLE_CLIENT_ID=seu-google-client-id-aqui
```

### 2. Configuração do Google OAuth (Opcional)

Se quiser usar login com Google:

1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione um existente
3. Habilite a API do Google+ 
4. Vá em "Credenciais" → "Criar credenciais" → "ID do cliente OAuth 2.0"
5. Configure os domínios autorizados:
   - `http://localhost:3000` (desenvolvimento)
   - `http://localhost:3001` (desenvolvimento alternativo)
6. Copie o Client ID e configure no arquivo `.env`

### 3. Backend
Certifique-se de que o backend está rodando na porta 5062:

```bash
# No diretório do backend
cd projeto/FastSurvey
dotnet run
```

## Testando o Login

### 1. Login Normal
- Use qualquer usuário e senha válidos do banco de dados
- O sistema redirecionará para `/home` ou `/admin` dependendo do tipo de usuário

### 2. Cadastro
- Preencha os campos: usuário, email e senha (mínimo 6 caracteres)
- Aceite os termos de uso
- O usuário será criado com tipo padrão

### 3. Login Google (se configurado)
- Clique no botão "Entrar com Google"
- Faça login com sua conta Google
- O sistema criará ou autenticará o usuário automaticamente

## Estrutura de Dados

### Resposta do Login
```json
{
  "loginId": 123,
  "usuario": "nome_do_usuario",
  "tipoUsuarioId": 13,
  "token": "jwt_token_aqui"
}
```

### Tipos de Usuário
- `13`: Usuário comum
- `15`: Administrador

### LocalStorage
O sistema salva automaticamente:
- `token`: JWT token de autenticação
- `userId`: ID do usuário logado
- `tipousuarioid`: Tipo do usuário

## Troubleshooting

### Erro "Usuário ou senha inválidos"
- Verifique se o backend está rodando
- Confirme se a URL da API está correta no `.env`
- Verifique se o usuário existe no banco de dados

### Erro "Falha na autenticação com Google"
- Verifique se o Google Client ID está configurado
- Confirme se os domínios estão autorizados no Google Cloud Console
- Verifique se a API do Google+ está habilitada

### Erro de CORS
- Certifique-se de que o backend permite requisições do frontend
- Verifique se as URLs estão configuradas corretamente

## Endpoints Utilizados

- `POST /api/login/Autenticar` - Login normal
- `POST /api/login/Cadastrar` - Cadastro de usuário
- `POST /api/login/GoogleAuth` - Login com Google
- `POST /api/login/EsqueciSenha` - Recuperação de senha

