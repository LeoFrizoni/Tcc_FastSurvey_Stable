# ⚡ ROTEIRO DE TESTE RÁPIDO - FASTSURVEY

## 🎯 **TESTES ESSENCIAIS (30 MINUTOS)**

### **1. 🔐 AUTENTICAÇÃO (5 min)**
- [ ] **Login Local**
  - [ ] Fazer login com credenciais válidas
  - [ ] Tentar login com credenciais inválidas
  - [ ] Verificar mensagens de erro

- [ ] **Cadastro**
  - [ ] Criar novo usuário
  - [ ] Tentar cadastrar email duplicado

### **2. 📝 CRIAÇÃO DE PESQUISA (10 min)**
- [ ] **Pesquisa Tradicional**
  - [ ] Criar nova pesquisa
  - [ ] Adicionar pergunta discursiva
  - [ ] Adicionar pergunta objetiva com opções
  - [ ] Salvar pesquisa

- [ ] **Pesquisa Interativa**
  - [ ] Criar pesquisa tipo Kahoot
  - [ ] Configurar tempo e pontuação

### **3. 📋 RESPONDER PESQUISA (5 min)**
- [ ] **Acesso**
  - [ ] Acessar link da pesquisa criada
  - [ ] Responder todas as perguntas
  - [ ] Enviar pesquisa

### **4. 📊 RESULTADOS (5 min)**
- [ ] **Visualização**
  - [ ] Verificar resultados no dashboard
  - [ ] Testar gráficos
  - [ ] Exportar dados (PDF/Excel)

### **5. 🎮 PESQUISA INTERATIVA (5 min)**
- [ ] **Sessão**
  - [ ] Iniciar sessão interativa
  - [ ] Conectar como participante
  - [ ] Responder perguntas em tempo real

---

## 🔍 **TESTES AVANÇADOS (OPCIONAL)**

### **6. 📱 RESPONSIVIDADE**
- [ ] Testar em mobile (375px)
- [ ] Testar em tablet (768px)
- [ ] Testar em desktop (1920px)

### **7. 🔒 SEGURANÇA**
- [ ] Tentar acessar área restrita sem login
- [ ] Verificar expiração de token
- [ ] Testar rate limiting

### **8. ⚙️ ADMINISTRAÇÃO**
- [ ] Acessar painel admin
- [ ] Listar usuários
- [ ] Verificar analytics global

---

## 🚨 **PROBLEMAS CRÍTICOS A VERIFICAR**

### **Funcionalidades Quebradas**
- [ ] Login não funciona
- [ ] Criação de pesquisa falha
- [ ] Respostas não são salvas
- [ ] Gráficos não carregam
- [ ] Pesquisa interativa não sincroniza

### **Problemas de UX**
- [ ] Interface não responsiva
- [ ] Mensagens de erro confusas
- [ ] Navegação confusa
- [ ] Carregamento muito lento

### **Problemas de Segurança**
- [ ] Acesso não autorizado
- [ ] Dados expostos
- [ ] Validação inadequada

---

## 📝 **CHECKLIST RÁPIDO**

### **✅ FUNCIONALIDADES PRINCIPAIS**
- [ ] Login/Cadastro funciona
- [ ] Criação de pesquisa funciona
- [ ] Resposta de pesquisa funciona
- [ ] Visualização de resultados funciona
- [ ] Pesquisa interativa funciona

### **✅ INTERFACE**
- [ ] Design responsivo
- [ ] Navegação intuitiva
- [ ] Mensagens claras
- [ ] Loading states funcionais

### **✅ PERFORMANCE**
- [ ] Carregamento rápido (< 3s)
- [ ] Navegação fluida
- [ ] Upload de arquivos funciona
- [ ] Múltiplos usuários simultâneos

### **✅ SEGURANÇA**
- [ ] Autenticação obrigatória
- [ ] Validação de dados
- [ ] Proteção contra ataques básicos

---

## 🐛 **COMO REPORTAR PROBLEMAS**

### **Template de Bug Report**
```
**Título:** [Descrição breve do problema]

**Severidade:** Alta/Média/Baixa

**Passos para reproduzir:**
1. [Passo 1]
2. [Passo 2]
3. [Passo 3]

**Resultado esperado:** [O que deveria acontecer]

**Resultado atual:** [O que está acontecendo]

**Ambiente:**
- Navegador: [Chrome/Firefox/Safari]
- Dispositivo: [Desktop/Mobile/Tablet]
- Sistema: [Windows/Mac/Linux]

**Screenshots:** [Se aplicável]
```

---

## ⚡ **DICAS PARA TESTE RÁPIDO**

1. **Use dados de teste simples** - Não perca tempo criando dados complexos
2. **Teste o fluxo principal** - Foque nas funcionalidades essenciais
3. **Documente problemas** - Anote tudo que não funciona
4. **Teste em diferentes navegadores** - Chrome, Firefox, Safari
5. **Teste responsividade** - Use DevTools para simular dispositivos
6. **Verifique mensagens de erro** - São indicadores importantes de problemas

---

## 📊 **MÉTRICA DE SUCESSO**

**✅ EXCELENTE:** Todas as funcionalidades principais funcionam
**⚠️ BOM:** Funcionalidades principais funcionam com pequenos problemas
**❌ PROBLEMÁTICO:** Funcionalidades críticas quebradas

---

**⏱️ Tempo Estimado:** 30-60 minutos  
**👤 Testador:** [Seu Nome]  
**📅 Data:** [Data do Teste]  

*Execute este roteiro sempre que houver mudanças significativas no sistema.*
