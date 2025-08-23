# Teste da Funcionalidade "Esqueci minha senha"

## ✅ Configurações Verificadas

### Backend (FastSurvey)
- ✅ Configurações de e-mail no `appsettings.json`
- ✅ Endpoint `/api/login/EsqueciSenha` implementado
- ✅ Serviço de e-mail registrado no DI
- ✅ Senha do app Gmail configurada

### Frontend
- ✅ Modal "Esqueci minha senha" implementado
- ✅ Endpoint correto configurado
- ✅ Estilos CSS adicionados

## 🧪 Como Testar

### 1. Teste Básico
1. Acesse a tela de login
2. Clique em "Esqueci minha senha"
3. Digite um e-mail válido que existe no banco
4. Clique em "Enviar"
5. Verifique se recebeu o e-mail

### 2. Verificar Logs do Backend
- Monitore os logs do backend para ver se o e-mail está sendo enviado
- Verifique se não há erros de SMTP

### 3. Testar com E-mail Inválido
- Digite um e-mail que não existe no banco
- Deve retornar sucesso (por segurança, não revela se o e-mail existe)

## 🔧 Configurações de E-mail

### Gmail App Password
- **E-mail**: fastsurvey@gmail.com
- **Senha do App**: inhxdiubxmtknigl
- **SMTP**: smtp.gmail.com
- **Porta**: 587
- **SSL**: Habilitado

### URL de Reset
- O link de reset será: `http://localhost:3000/reset-password?token=TOKEN`

## 🚨 Possíveis Problemas

### 1. E-mail não chega
- Verifique se a senha do app está correta
- Confirme se o Gmail permite "apps menos seguros"
- Verifique os logs do backend

### 2. Erro de SMTP
- Verifique as configurações no `appsettings.json`
- Confirme se o Gmail não bloqueou o acesso

### 3. Token não funciona
- Verifique se a tabela `Tokens` existe no banco
- Confirme se o token não expirou (24 horas)

## 📧 Estrutura do E-mail

O e-mail enviado contém:
- Assunto: "Reset de Senha - FastSurvey"
- Corpo HTML com:
  - Saudação personalizada
  - Link para reset de senha
  - Informação sobre expiração (24h)
  - Aviso de segurança

## 🔗 Próximos Passos

1. **Implementar página de reset de senha** no frontend
2. **Testar o endpoint `/api/login/ResetarSenha`**
3. **Adicionar validação de força da senha**
4. **Implementar confirmação de e-mail no cadastro**
