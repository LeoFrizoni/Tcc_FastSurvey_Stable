# 🧹 Scripts de Limpeza de Cache - FastSurvey Frontend

Este projeto inclui scripts automatizados para limpar o cache do React e evitar problemas de cache durante o desenvolvimento.

## 📋 Scripts Disponíveis

### **1. `npm run dev` (Recomendado)**
```bash
npm run dev
```
- ✅ **Limpa cache automaticamente** antes de iniciar
- ✅ **Limpa cache automaticamente** quando encerra (Ctrl+C)
- ✅ Inicia o servidor de desenvolvimento
- 🎯 **Use este comando para desenvolvimento**

### **2. `npm run start:clean`**
```bash
npm run start:clean
```
- ✅ Limpa cache antes de iniciar
- ✅ Inicia o servidor normalmente
- ⚠️ **NÃO limpa automaticamente ao encerrar**

### **3. `npm run clean`**
```bash
npm run clean
```
- ✅ Limpa apenas o cache do React
- ❌ Não inicia o servidor

### **4. `npm run clean:all`**
```bash
npm run clean:all
```
- ✅ Limpa cache do React
- ✅ Limpa cache do npm
- ❌ Não inicia o servidor

## 🔧 Como Funciona

### **Cache Limpo Automaticamente:**
- `node_modules/.cache` (Cache do React)
- `build` (Builds antigos)
- `.next` (Cache do Next.js, se aplicável)
- `.nuxt` (Cache do Nuxt, se aplicável)
- `.cache` (Cache geral)

### **Quando o Cache é Limpo:**
1. **Antes de iniciar** o servidor (com `npm run dev`)
2. **Após encerrar** o servidor (Ctrl+C com `npm run dev`)
3. **Manual** com `npm run clean`

## 🚀 Uso Recomendado

### **Para Desenvolvimento:**
```bash
# Sempre use este comando
npm run dev
```

### **Se tiver problemas de cache:**
```bash
# Limpa tudo e reinicia
npm run clean:all
npm run dev
```

### **Para produção:**
```bash
npm run build
```

## 🐛 Solução de Problemas

### **Problema: Endpoints antigos ainda aparecem**
```bash
# Solução 1: Use o script automático
npm run dev

# Solução 2: Limpe manualmente
npm run clean:all
npm start
```

### **Problema: Erros de compilação**
```bash
# Limpe tudo e reinstale dependências
npm run clean:all
rm -rf node_modules
npm install
npm run dev
```

## 📝 Notas

- O script `dev-start.js` captura sinais de encerramento (SIGINT, SIGTERM)
- A limpeza automática acontece 1 segundo após encerrar o servidor
- Todos os scripts são compatíveis com Windows, Mac e Linux
- Os scripts não afetam arquivos de código, apenas cache

## 🎯 Benefícios

1. **Sem problemas de cache** durante desenvolvimento
2. **Inicialização mais rápida** após mudanças
3. **Menos erros** de endpoints antigos
4. **Desenvolvimento mais fluido**
