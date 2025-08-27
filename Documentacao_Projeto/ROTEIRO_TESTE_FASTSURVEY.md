# 🧪 ROTEIRO DE TESTE COMPLETO - FASTSURVEY

## 📋 **INFORMAÇÕES GERAIS**

**Sistema:** FastSurvey - Plataforma de Pesquisas Interativas  
**Versão:** 1.0.0  
**Data:** Dezembro 2024  
**Ambiente de Teste:** Desenvolvimento/Produção  
**Testador:** [Nome do Testador]  

---

## 🎯 **OBJETIVOS DO TESTE**

- ✅ Validar funcionalidades principais do sistema
- ✅ Verificar usabilidade e experiência do usuário
- ✅ Testar responsividade em diferentes dispositivos
- ✅ Validar segurança e autenticação
- ✅ Verificar performance e estabilidade
- ✅ Testar integração entre frontend e backend

---

## 🛠️ **PRÉ-REQUISITOS**

### **Ambiente de Teste**
- [ ] Backend ASP.NET Core rodando (porta 5000/5001)
- [ ] Frontend React rodando (porta 3000)
- [ ] Banco de dados PostgreSQL configurado
- [ ] Google OAuth configurado (opcional)
- [ ] Navegadores: Chrome, Firefox, Safari, Edge
- [ ] Dispositivos: Desktop, Tablet, Mobile

### **Dados de Teste**
- [ ] Usuário administrador criado
- [ ] Usuário normal criado
- [ ] Usuário premium criado
- [ ] Pesquisas de exemplo criadas
- [ ] Dados de teste para respostas

---

## 🔐 **TESTE 1: AUTENTICAÇÃO E LOGIN**

### **1.1 Login Local**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Login válido | 1. Acessar página de login<br>2. Inserir email válido<br>3. Inserir senha válida<br>4. Clicar em "Entrar" | Usuário logado e redirecionado para dashboard | ⬜ | |
| Email inválido | 1. Inserir email inexistente<br>2. Inserir senha qualquer<br>3. Tentar login | Mensagem de erro: "Credenciais inválidas" | ⬜ | |
| Senha incorreta | 1. Inserir email válido<br>2. Inserir senha incorreta<br>3. Tentar login | Mensagem de erro: "Credenciais inválidas" | ⬜ | |
| Campos vazios | 1. Deixar campos em branco<br>2. Tentar login | Validação visual nos campos obrigatórios | ⬜ | |
| Rate limiting | 1. Tentar login 6 vezes seguidas<br>2. Verificar bloqueio | Bloqueio temporário após 5 tentativas | ⬜ | |

### **1.2 Login Google OAuth**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Login Google válido | 1. Clicar em "Login com Google"<br>2. Autorizar aplicação<br>3. Verificar redirecionamento | Usuário logado via Google | ⬜ | |
| Primeiro acesso Google | 1. Login com conta Google nova<br>2. Verificar criação automática | Conta criada automaticamente | ⬜ | |
| Cancelar autorização | 1. Clicar em "Login com Google"<br>2. Cancelar na tela Google | Retorna para página de login | ⬜ | |

### **1.3 Cadastro de Usuário**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Cadastro válido | 1. Clicar em "Cadastrar"<br>2. Preencher dados válidos<br>3. Confirmar cadastro | Usuário criado e logado | ⬜ | |
| Email duplicado | 1. Tentar cadastrar email existente | Mensagem: "Email já cadastrado" | ⬜ | |
| Senha fraca | 1. Inserir senha simples | Validação de força da senha | ⬜ | |
| Validação de campos | 1. Deixar campos obrigatórios vazios | Validação visual nos campos | ⬜ | |

### **1.4 Recuperação de Senha**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Email válido | 1. Clicar em "Esqueci minha senha"<br>2. Inserir email válido<br>3. Verificar email | Email de recuperação enviado | ⬜ | |
| Email inexistente | 1. Inserir email inexistente | Mensagem de erro apropriada | ⬜ | |

---

## 🏠 **TESTE 2: DASHBOARD E NAVEGAÇÃO**

### **2.1 Dashboard Principal**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Carregamento inicial | 1. Fazer login<br>2. Verificar dashboard | Dashboard carrega com estatísticas | ⬜ | |
| Estatísticas | 1. Verificar cards de estatísticas | Dados corretos exibidos | ⬜ | |
| Pesquisas recentes | 1. Verificar lista de pesquisas recentes | Lista atualizada exibida | ⬜ | |
| Navegação lateral | 1. Testar menu lateral | Navegação funcional | ⬜ | |

### **2.2 Responsividade**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Desktop (1920x1080) | 1. Acessar em desktop | Layout completo exibido | ⬜ | |
| Tablet (768x1024) | 1. Acessar em tablet | Layout adaptado | ⬜ | |
| Mobile (375x667) | 1. Acessar em mobile | Menu hambúrguer funcional | ⬜ | |
| Orientação | 1. Rotacionar dispositivo | Layout se adapta | ⬜ | |

---

## 📝 **TESTE 3: CRIAÇÃO DE PESQUISAS**

### **3.1 Pesquisa Tradicional**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Criar pesquisa básica | 1. Clicar em "Nova Pesquisa"<br>2. Preencher título e descrição<br>3. Salvar | Pesquisa criada com sucesso | ⬜ | |
| Adicionar pergunta discursiva | 1. Adicionar pergunta<br>2. Selecionar tipo "Discursiva"<br>3. Configurar | Pergunta adicionada corretamente | ⬜ | |
| Adicionar pergunta objetiva | 1. Adicionar pergunta<br>2. Selecionar tipo "Objetiva"<br>3. Adicionar opções | Opções configuradas | ⬜ | |
| Adicionar pergunta múltipla | 1. Adicionar pergunta<br>2. Selecionar tipo "Múltipla"<br>3. Configurar | Múltiplas opções habilitadas | ⬜ | |
| Upload de anexo | 1. Adicionar pergunta com anexo<br>2. Fazer upload de arquivo | Arquivo anexado corretamente | ⬜ | |
| Reordenar perguntas | 1. Arrastar perguntas | Ordem alterada | ⬜ | |
| Preview da pesquisa | 1. Clicar em "Visualizar" | Preview funcional | ⬜ | |

### **3.2 Pesquisa Interativa (Kahoot)**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Criar pesquisa interativa | 1. Selecionar tipo "Interativa"<br>2. Configurar tempo por pergunta<br>3. Configurar pontuação | Pesquisa interativa criada | ⬜ | |
| Configurar gamificação | 1. Definir pontos por resposta<br>2. Configurar bônus de tempo<br>3. Configurar streak bonus | Gamificação configurada | ⬜ | |

### **3.3 Organização em Pastas**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Criar pasta | 1. Clicar em "Nova Pasta"<br>2. Definir nome<br>3. Salvar | Pasta criada | ⬜ | |
| Mover pesquisa | 1. Selecionar pesquisa<br>2. Mover para pasta | Pesquisa movida | ⬜ | |
| Renomear pasta | 1. Clicar em editar pasta<br>2. Alterar nome | Nome alterado | ⬜ | |
| Excluir pasta | 1. Selecionar pasta<br>2. Excluir | Pasta excluída (com confirmação) | ⬜ | |

---

## 🎮 **TESTE 4: PESQUISA INTERATIVA (KAHOOT)**

### **4.1 Iniciar Sessão**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Iniciar sessão | 1. Selecionar pesquisa interativa<br>2. Clicar em "Iniciar" | Código de acesso gerado | ⬜ | |
| Modo apresentação | 1. Verificar tela de apresentação | Tela de apresentação exibida | ⬜ | |
| Código de acesso | 1. Verificar código exibido | Código visível e legível | ⬜ | |

### **4.2 Participação**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Entrar na sessão | 1. Acessar link de participação<br>2. Inserir código<br>3. Inserir nome | Participante registrado | ⬜ | |
| Responder pergunta | 1. Aguardar pergunta<br>2. Selecionar resposta<br>3. Confirmar | Resposta enviada | ⬜ | |
| Ver ranking | 1. Aguardar resultados<br>2. Verificar ranking | Ranking atualizado | ⬜ | |
| Múltiplos participantes | 1. Conectar vários dispositivos<br>2. Verificar sincronização | Todos sincronizados | ⬜ | |

### **4.3 Gamificação**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Pontuação por resposta | 1. Responder corretamente<br>2. Verificar pontos | Pontos atribuídos | ⬜ | |
| Bônus de tempo | 1. Responder rapidamente<br>2. Verificar bônus | Bônus aplicado | ⬜ | |
| Streak bonus | 1. Acertar várias seguidas<br>2. Verificar streak | Streak contabilizado | ⬜ | |

---

## 📋 **TESTE 5: RESPONDER PESQUISAS**

### **5.1 Acesso à Pesquisa**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Link direto | 1. Acessar link da pesquisa | Página da pesquisa carrega | ⬜ | |
| QR Code | 1. Escanear QR Code | Redirecionamento correto | ⬜ | |
| Identificação obrigatória | 1. Pesquisa com identificação<br>2. Preencher dados | Dados coletados | ⬜ | |
| Pesquisa anônima | 1. Pesquisa sem identificação | Acesso direto às perguntas | ⬜ | |

### **5.2 Resposta às Perguntas**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Pergunta discursiva | 1. Digitar resposta<br>2. Avançar | Resposta salva | ⬜ | |
| Pergunta objetiva | 1. Selecionar opção<br>2. Avançar | Opção selecionada | ⬜ | |
| Pergunta múltipla | 1. Selecionar múltiplas opções<br>2. Avançar | Opções selecionadas | ⬜ | |
| Upload de arquivo | 1. Fazer upload<br>2. Verificar arquivo | Arquivo enviado | ⬜ | |
| Validação obrigatória | 1. Tentar avançar sem responder | Validação exibida | ⬜ | |
| Navegação entre perguntas | 1. Usar botões anterior/próximo | Navegação funcional | ⬜ | |

### **5.3 Finalização**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Enviar pesquisa | 1. Responder todas as perguntas<br>2. Clicar em "Enviar" | Pesquisa enviada | ⬜ | |
| Confirmação | 1. Verificar página de agradecimento | Confirmação exibida | ⬜ | |
| Dados salvos | 1. Verificar no dashboard | Respostas aparecem | ⬜ | |

---

## 📊 **TESTE 6: ANÁLISE DE RESULTADOS**

### **6.1 Visualização de Dados**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Gráficos | 1. Acessar resultados<br>2. Verificar gráficos | Gráficos renderizados | ⬜ | |
| Tabelas | 1. Alternar para visualização tabular | Dados em tabela | ⬜ | |
| Filtros | 1. Aplicar filtros de data<br>2. Verificar resultados | Filtros funcionais | ⬜ | |
| Estatísticas | 1. Verificar estatísticas gerais | Estatísticas corretas | ⬜ | |

### **6.2 Exportação**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Exportar PDF | 1. Clicar em "Exportar PDF" | PDF gerado e baixado | ⬜ | |
| Exportar Excel | 1. Clicar em "Exportar Excel" | Arquivo Excel baixado | ⬜ | |
| Exportar CSV | 1. Clicar em "Exportar CSV" | Arquivo CSV baixado | ⬜ | |

### **6.3 Analytics Avançados**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Dashboard analytics | 1. Acessar analytics<br>2. Verificar métricas | Métricas exibidas | ⬜ | |
| Relatórios | 1. Gerar relatórios<br>2. Verificar dados | Relatórios corretos | ⬜ | |

---

## ⚙️ **TESTE 7: ADMINISTRAÇÃO**

### **7.1 Gerenciamento de Usuários**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Listar usuários | 1. Acessar painel admin<br>2. Verificar lista | Lista de usuários exibida | ⬜ | |
| Criar usuário | 1. Clicar em "Novo Usuário"<br>2. Preencher dados<br>3. Salvar | Usuário criado | ⬜ | |
| Editar usuário | 1. Selecionar usuário<br>2. Alterar dados<br>3. Salvar | Dados alterados | ⬜ | |
| Alterar tipo | 1. Alterar tipo de usuário<br>2. Verificar permissões | Permissões atualizadas | ⬜ | |
| Excluir usuário | 1. Selecionar usuário<br>2. Excluir | Usuário excluído | ⬜ | |

### **7.2 Analytics Global**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Estatísticas gerais | 1. Acessar analytics global<br>2. Verificar métricas | Métricas corretas | ⬜ | |
| Relatórios periódicos | 1. Gerar relatórios<br>2. Verificar agendamento | Relatórios agendados | ⬜ | |

### **7.3 Configurações do Sistema**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Configurações gerais | 1. Acessar configurações<br>2. Alterar parâmetros<br>3. Salvar | Configurações salvas | ⬜ | |
| Configurações de email | 1. Configurar SMTP<br>2. Testar envio | Email configurado | ⬜ | |
| Configurações OAuth | 1. Configurar Google OAuth<br>2. Testar login | OAuth funcionando | ⬜ | |

---

## 🔒 **TESTE 8: SEGURANÇA**

### **8.1 Autenticação e Autorização**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Token expirado | 1. Aguardar expiração do token<br>2. Tentar operação | Redirecionamento para login | ⬜ | |
| Acesso não autorizado | 1. Tentar acessar área restrita | Acesso negado | ⬜ | |
| CSRF protection | 1. Tentar requisição maliciosa | Proteção ativa | ⬜ | |
| XSS protection | 1. Inserir script malicioso | Script não executado | ⬜ | |

### **8.2 Validação de Dados**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| SQL Injection | 1. Tentar injeção SQL | Dados sanitizados | ⬜ | |
| Upload malicioso | 1. Tentar upload de arquivo malicioso | Upload bloqueado | ⬜ | |
| Validação de entrada | 1. Inserir dados inválidos | Validação aplicada | ⬜ | |

---

## 📱 **TESTE 9: PERFORMANCE E ESTABILIDADE**

### **9.1 Performance**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Carregamento inicial | 1. Medir tempo de carregamento | < 3 segundos | ⬜ | |
| Navegação entre páginas | 1. Medir tempo de transição | < 1 segundo | ⬜ | |
| Upload de arquivos | 1. Testar upload de arquivo grande | Upload funcional | ⬜ | |
| Múltiplos usuários | 1. Simular múltiplos acessos | Sistema estável | ⬜ | |

### **9.2 Estabilidade**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Sessão longa | 1. Manter sessão ativa por 1 hora | Sessão mantida | ⬜ | |
| Múltiplas abas | 1. Abrir múltiplas abas | Funcionamento correto | ⬜ | |
| Recarregamento | 1. Recarregar páginas | Estado mantido | ⬜ | |

---

## 🌐 **TESTE 10: INTEGRAÇÃO E API**

### **10.1 APIs REST**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| GET /api/pesquisas | 1. Fazer requisição GET | Lista de pesquisas retornada | ⬜ | |
| POST /api/pesquisas | 1. Criar pesquisa via API | Pesquisa criada | ⬜ | |
| PUT /api/pesquisas/{id} | 1. Atualizar pesquisa via API | Pesquisa atualizada | ⬜ | |
| DELETE /api/pesquisas/{id} | 1. Excluir pesquisa via API | Pesquisa excluída | ⬜ | |

### **10.2 Autenticação API**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Token válido | 1. Fazer requisição com token | Acesso permitido | ⬜ | |
| Token inválido | 1. Fazer requisição sem token | Acesso negado (401) | ⬜ | |
| Token expirado | 1. Usar token expirado | Acesso negado (401) | ⬜ | |

---

## 🐛 **TESTE 11: TRATAMENTO DE ERROS**

### **11.1 Erros de Rede**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Sem conexão | 1. Desconectar internet<br>2. Tentar operação | Mensagem de erro apropriada | ⬜ | |
| Timeout | 1. Simular timeout<br>2. Verificar tratamento | Timeout tratado | ⬜ | |

### **11.2 Erros de Validação**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Dados inválidos | 1. Enviar dados inválidos | Mensagens de erro claras | ⬜ | |
| Campos obrigatórios | 1. Deixar campos vazios | Validação visual | ⬜ | |

### **11.3 Erros do Sistema**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Erro 500 | 1. Simular erro interno | Página de erro amigável | ⬜ | |
| Erro 404 | 1. Acessar URL inexistente | Página 404 personalizada | ⬜ | |

---

## 📋 **TESTE 12: USABILIDADE E UX**

### **12.1 Interface do Usuário**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Design responsivo | 1. Testar em diferentes resoluções | Layout adaptativo | ⬜ | |
| Acessibilidade | 1. Testar com leitor de tela | Acessível | ⬜ | |
| Contraste | 1. Verificar contraste de cores | Contraste adequado | ⬜ | |
| Navegação intuitiva | 1. Testar fluxo de navegação | Navegação clara | ⬜ | |

### **12.2 Feedback do Usuário**
| **Cenário** | **Passos** | **Resultado Esperado** | **Status** | **Observações** |
|-------------|------------|------------------------|------------|-----------------|
| Loading states | 1. Verificar indicadores de carregamento | Loading exibido | ⬜ | |
| Mensagens de sucesso | 1. Completar operação<br>2. Verificar feedback | Feedback positivo | ⬜ | |
| Mensagens de erro | 1. Provocar erro<br>2. Verificar mensagem | Mensagem clara | ⬜ | |

---

## 📝 **RELATÓRIO DE TESTES**

### **Resumo Executivo**
- **Total de Testes:** [X] casos de teste
- **Testes Passaram:** [X] (XX%)
- **Testes Falharam:** [X] (XX%)
- **Testes Bloqueados:** [X] (XX%)

### **Principais Problemas Encontrados**
1. **Problema 1:** [Descrição]
   - **Severidade:** Alta/Média/Baixa
   - **Impacto:** [Descrição do impacto]
   - **Recomendação:** [Sugestão de correção]

2. **Problema 2:** [Descrição]
   - **Severidade:** Alta/Média/Baixa
   - **Impacto:** [Descrição do impacto]
   - **Recomendação:** [Sugestão de correção]

### **Recomendações Gerais**
1. [Recomendação 1]
2. [Recomendação 2]
3. [Recomendação 3]

### **Próximos Passos**
1. [ ] Corrigir problemas críticos
2. [ ] Reexecutar testes falhados
3. [ ] Testes de regressão
4. [ ] Testes de aceitação do usuário

---

## 📊 **MÉTRICAS DE QUALIDADE**

| **Métrica** | **Meta** | **Resultado** | **Status** |
|-------------|----------|---------------|------------|
| Cobertura de Testes | 90% | [X]% | ⬜ |
| Taxa de Sucesso | 95% | [X]% | ⬜ |
| Tempo de Resposta | < 3s | [X]s | ⬜ |
| Disponibilidade | 99.9% | [X]% | ⬜ |

---

**📅 Data de Execução:** [DD/MM/AAAA]  
**👤 Executado por:** [Nome do Testador]  
**✅ Aprovado por:** [Nome do Aprovador]  

---

*Este roteiro de teste foi criado especificamente para a aplicação FastSurvey e deve ser atualizado conforme novas funcionalidades sejam implementadas.*
