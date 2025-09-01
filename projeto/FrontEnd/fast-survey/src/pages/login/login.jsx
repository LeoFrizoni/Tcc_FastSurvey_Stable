// src/pages/login/login.jsx
import React, { useState, useRef, useEffect } from "react";
import styles from "./login.module.css";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { Eye, EyeOff } from "lucide-react";
import GoogleLoginButton from "../../components/GoogleLoginButton";
import api from "../../lib/api";
import env from "../../config/env";
import { checkUsernameAvailability } from "../../helpers/validateEmail";

/* ===================== Endpoints (mantidos) ===================== */
const ENDPOINTS = {
  login: "/api/login/Autenticar",
  register: "/api/login/Cadastrar",
  googleAuthPreferred: "/api/login/GoogleAuth",
  googleAuthFallback: "/api/Login/GoogleAuth",
};

const RESET_PASSWORD_ENDPOINTS = [
  "/api/login/EsqueciSenha",
];

const RESEND_VERIFICATION_ENDPOINTS = [
  "/api/login/ReenviarConfirmacao",
];

/* ===== Termos de Uso (texto longo; exibido com pre-wrap para manter formatação) ===== */
const TERMS_TEXT = `# TERMOS DE USO - FASTSURVEY

## 1. ACEITAÇÃO DOS TERMOS

Estes Termos de Uso ("Termos") se aplicam a todos os usuários da plataforma FastSurvey (a "Plataforma"), incluindo usuários gratuitos, premium e administradores, e a todas as partes que visitam o website FastSurvey. Conforme usado nestes Termos, "Você" e "você" inclui todas essas partes, sejam elas atuando em seu próprio nome, ou em nome de ou como parte de uma empresa ou outra entidade.

Ao visitar o Website ou usar a Plataforma, você está representando e afirmando que leu, entendeu e concordou que está vinculado a estes Termos, e é de idade legal para aceitar e concordar com estes Termos. Se você não concordar com estes Termos, não tem permissão para usar a Plataforma FastSurvey.

Os termos "nós", "nos" e "nosso" usados abaixo se referem ao FastSurvey.

## 2. QUEM PODE USAR A PLATAFORMA / ASSINATURAS

Exceto para nossa versão gratuita da Plataforma FastSurvey, seu direito de usar a Plataforma depende do pagamento pontual das taxas necessárias. Usuários premium e administradores obtêm uma assinatura para usar a Plataforma uma vez que paguem; usuários gratuitos obtêm nenhuma assinatura, mas ainda podem usar a Plataforma. Todos os usuários devem cumprir estes termos.

Você concorda em usar a Plataforma apenas conforme permitido nestes Termos.

## 3. RESTRIÇÕES DE USO

Você aceita e assume total responsabilidade pelo seu uso da Plataforma.

Você não pode compartilhar suas informações de login/credenciais ou permitir que qualquer outra pessoa as use para acessar a Plataforma.

Você concorda que sempre cumprirá todas as leis aplicáveis em conexão com seu uso da Plataforma. Isso significa não violar qualquer lei aplicável, direito legal ou proteção, incluindo, mas não se limitando aos direitos de privacidade de terceiros e direitos de propriedade intelectual.

A Plataforma NÃO pode ser usada para, ou em conexão com qualquer um dos seguintes, qualquer um ou todos os quais podem resultar na suspensão temporária ou terminação permanente da sua conta, a nosso critério exclusivo:

- **Solicitação de email, Mass Emailing, Spamming, Phishing e similares**
- **Violação dos direitos de privacidade de qualquer pessoa**
- **Fraude financeira/monetária ou esquemas**
- **Fraude de computador ou outros crimes de computador**
- **Difamação, discriminação ou assédio**
- **Financiamento, encorajamento ou facilitação de atividades criminais ou ilegais**
- **Promoção, encorajamento ou solicitação de qualquer forma de violência ou dano a qualquer pessoa**
- **Coleta de informações de cartão de crédito ou credenciais de login de terceiros para outros sites ou plataformas**
- **Coleta de informações pessoais altamente sensíveis como números de segurança social, cartões de identidade nacional ou números, e similares, sem o que consideramos ser uma razão válida para fazê-lo**
- **Venda, aluguel, arrendamento ou divulgação ou fornecimento de informações pessoais coletadas em suas pesquisas para terceiros**
- **Pesquisas que incluem campos para o respondente fornecer informações pessoais de qualquer terceiro onde o respondente não tem a permissão do terceiro para fornecer tais informações**
- **Promoção de seus ou outros produtos ou serviços de terceiros em nossa plataforma ou website**
- **Promoção de produtos ou serviços em plataformas de mídia social ou outros websites onde tais ações violam os termos de uso ou diretrizes da comunidade de tais plataformas ou websites**
- **Esquemas de ganho de dinheiro ou outros envolvendo serviços profissionais legais, médicos ou outros**
- **Inclusão de conteúdo protegido por direitos autorais ou marca registrada de terceiros ou segredos comerciais em suas pesquisas sem a permissão dessa parte**
- **Encorajamento de violência, bullying ou dano a outros**
- **Coleta, coleta ou solicitação ou sugestão de que os respondentes da pesquisa forneçam conteúdo pornográfico ou sexualmente explícito, ou promoção ou encorajamento de prostituição ou outras atividades envolvendo a troca de serviços sexuais como parte de qualquer transação**
- **Qualquer outra atividade proibida sob estes Termos ou sob a lei aplicável**

Você concorda que temos o direito absoluto e incondicional de remover qualquer pesquisa que acreditamos, a nosso critério exclusivo, estar sendo usada ou pode ser usada em conexão com qualquer dos propósitos ou atividades proibidos acima, seja tal uso por você ou outros, como respondentes de pesquisa que podem usar suas pesquisas. Em casos onde removemos uma pesquisa, podemos, a nosso critério exclusivo, também desabilitar seu acesso a quaisquer envios passados ou futuros para tais pesquisas.

Você e seus agentes aqui liberam para sempre o FastSurvey de qualquer e toda responsabilidade por quaisquer erros e violações de nossos termos ou da lei cometidos por você relacionados ao seu uso da Plataforma.

Você concorda em não reproduzir, duplicar, fazer engenharia reversa, copiar, vender, revender ou explorar para quaisquer propósitos comerciais a Plataforma ou qualquer parte dela ou oferecer ou vender o direito de usar a Plataforma.

Você concorda que, se em nosso critério exclusivo determinarmos que suas pesquisas estão coletando ou pretendem coletar informações altamente sensíveis, podemos desabilitar as pesquisas e pedir informações de qualquer natureza que consideremos apropriadas para determinar se você parece ter uma razão válida para coletar tais informações, e que se não estivermos satisfeitos após essa revisão que você tem uma razão válida, as pesquisas permanecerão desabilitadas.

## 4. PESQUISAS E ENVIOS

Você aceita e tem total responsabilidade pelas pesquisas, tabelas, páginas de envio e outro conteúdo que você cria ou que é gerado conforme você usa a Plataforma. Quando você torna tal conteúdo "público", ou define as configurações da sua conta para permitir que o conteúdo se torne público, ou você não muda uma configuração padrão que permitiria que uma pesquisa, uma tabela ou outro conteúdo que é gerado conforme você usa a Plataforma seja tornado público, você reconhece e concorda que o conteúdo de fato estará disponível ao público em nossos websites. Ao permitir que tal conteúdo se torne público, você concede ao FastSurvey uma licença mundial e totalmente sublicenciável para usar, distribuir, reproduzir, modificar, adaptar, publicar, traduzir, executar publicamente e exibir publicamente tal conteúdo em nossos websites. É claro que você pode escolher aplicar configurações à sua conta para tornar ou manter as pesquisas privadas.

Você concorda que não fará qualquer reivindicação de direitos autorais ou outras reivindicações de propriedade intelectual ou direitos em pesquisas que você constrói ou cria usando a Plataforma contra o FastSurvey, e que você renuncia a quaisquer tais reivindicações legais contra o FastSurvey relacionadas a tais pesquisas. Isso inclui pesquisas inteiras e partes de tais pesquisas. Você reconhece e concorda que o FastSurvey pode exibir suas pesquisas em nossos websites e plataformas, e que podemos usar suas pesquisas para os propósitos de melhorar o construtor de pesquisas, para aprender sobre como você e outras pessoas usam, constroem e criam e enviam pesquisas, para ensinar nossa equipe, sistemas e produtos sobre tais assuntos, e para outros propósitos.

## 5. CRIAÇÃO DE CONTA E SEGURANÇA

Você deve fornecer um endereço de email válido e qualquer outra informação solicitada para completar totalmente o processo de inscrição e criar um login. Você só pode criar um login separado para tantos Usuários quanto seu plano atual do FastSurvey permitir. O compartilhamento das credenciais de login da sua Conta FastSurvey é estritamente proibido. Você é responsável por manter a segurança da conta de cada Usuário, nome de usuário e senha e por garantir que cada Usuário associado à sua Conta FastSurvey cumpra estes Termos. Você não pode acessar a Plataforma FastSurvey através de métodos automatizados, como usar bots ou código de computador para chamar ou fazer ping na Plataforma ou nosso website. Se você precisar de um plano multi-usuário, entre em contato com o FastSurvey Enterprise para assistência.

## 6. PAGAMENTOS, RENOVAÇÕES, ETC.

### A. Pagamentos
Cobramos impostos sobre vendas em assinaturas onde somos obrigados a fazê-lo sob a lei aplicável. Nenhum outro imposto será cobrado. Você é responsável pelo pagamento de quaisquer e todos os impostos, taxas e deveres, incluindo quaisquer impostos sobre vendas ou valor agregado e impostos e deveres similares, que podem ser impostos a você por qualquer autoridade governante em qualquer jurisdição em conexão com sua assinatura.

O FastSurvey oferece pagamentos através de processadores de pagamento de terceiros. Ao fazer pagamentos para nós pelos serviços do FastSurvey, você indica que revisou estes Termos e a política de privacidade do processador de pagamento que processa seu pagamento para nós.

Se você usar um cartão de crédito para pagar sua assinatura do FastSurvey, você representa e garante que as informações do cartão de crédito que você fornece estão corretas e que você notificará prontamente qualquer mudança em tais informações do cartão de crédito. Você concorda que se seu pagamento com cartão de crédito não puder ser processado por qualquer razão, o FastSurvey pode suspender ou cancelar sua assinatura do FastSurvey.

### B. Renovação Automática
Para assinaturas, você será cobrado antecipadamente em uma base periódica recorrente. Sua assinatura do FastSurvey será automaticamente renovada no final de cada ciclo de cobrança até que você cancele sua assinatura ou faça downgrade para um plano gratuito. Você também pode nos enviar uma solicitação para fazer downgrade para o plano gratuito aqui. Desligar a renovação automática impede que seu método de pagamento seja cobrado na sua próxima data de cobrança, ou se você pagar por fatura, impede que sua próxima fatura seja emitida.

### C. Upgrades/Downgrades
Fazer upgrade do seu plano, como ir de um plano gratuito para um plano premium, adicionará ou aumentará coisas como o número incluído/permitido de pesquisas incluídas, envios, espaço disponível, armazenamento, etc. Fazer downgrade do seu plano resultará, a partir da sua próxima data de cobrança, em uma diminuição de algumas ou todas essas coisas e pode fazer você perder pesquisas e envios. Você pode deletar sua conta inteiramente clicando em "Deletar Minha Conta" na página de Configurações da Conta.

### D. Política de Reembolso de 30 Dias
Se por qualquer razão você cancelar sua conta dentro de 30 dias do seu pagamento feito diretamente ao FastSurvey, você pode solicitar e receber um reembolso enviando uma solicitação à Equipe de Suporte do FastSurvey. Reembolsos por razões outras que não o cancelamento da conta dentro dos 30 dias estão a critério exclusivo do FastSurvey.

## 7. USO DE DADOS

Você aceita e concorda que tem total responsabilidade pelas informações, dados e conteúdo (coletivamente "Dados") que você recebe ou coleta de ou nas pesquisas que você cria ou usa em conexão com a Plataforma, e pelo que você faz com esses Dados. Você concorda que o FastSurvey não é responsável por e não possui nenhum desses Dados.

Você aqui nos autoriza a acessar, usar e exibir Dados para o propósito e na extensão necessária para fornecer a Plataforma para você, suporte ao cliente para você, para proteger os Dados, para proteger nossos recursos online e de computador de ataques cibernéticos ilegais, e para cumprir nossas obrigações legais.

Veja nossa política de privacidade para mais informações sobre como coletamos, usamos e divulgamos informações pessoais e privadas às quais temos acesso em conexão com nosso fornecimento e operação da Plataforma - incluindo suas informações pessoais. Não modificaremos Dados ou suas informações pessoais, ou venderemos ou alugaremos para qualquer outra parte.

Se você coletar dados pessoais de ou de residentes da UE, você deve usar o recurso EU Safe Forms da Plataforma FastSurvey.

## 8. PESQUISAS INTERATIVAS E GAMIFICAÇÃO

### A. Sessões Interativas
O FastSurvey oferece funcionalidade de pesquisas interativas (estilo Kahoot) que permite criar sessões ao vivo com participantes. Ao usar esta funcionalidade, você concorda em:

- Usar as sessões interativas apenas para propósitos educacionais, de treinamento ou de entretenimento legítimos
- Não usar a funcionalidade para assédio, bullying ou qualquer forma de comportamento inadequado
- Respeitar os direitos de privacidade dos participantes
- Não coletar informações pessoais sensíveis através de sessões interativas sem consentimento explícito

### B. Sistema de Pontuação e Ranking
O sistema de gamificação inclui pontuação e ranking em tempo real. Você concorda em:

- Não manipular o sistema de pontuação
- Não usar múltiplas contas para inflar pontuações
- Respeitar a integridade do sistema de ranking
- Não usar bots ou scripts automatizados para participar de sessões

### C. Chat em Tempo Real
As sessões interativas podem incluir chat em tempo real. Você concorda em:

- Não usar linguagem ofensiva, abusiva ou inadequada
- Não enviar spam ou mensagens repetitivas
- Respeitar outros participantes
- Não compartilhar informações pessoais de outros participantes

## 9. QR CODES E COMPARTILHAMENTO

### A. Geração de QR Codes
O FastSurvey gera automaticamente QR Codes para suas pesquisas. Você concorda em:

- Usar QR Codes apenas para pesquisas legítimas
- Não usar QR Codes para direcionar para conteúdo malicioso
- Respeitar a configuração de expiração dos QR Codes
- Não compartilhar QR Codes de pesquisas privadas sem autorização

### B. Compartilhamento de Pesquisas
Ao compartilhar suas pesquisas, você concorda em:

- Não compartilhar pesquisas que violem estes Termos
- Respeitar a privacidade dos respondentes
- Não usar links de pesquisa para spam ou phishing
- Informar respondentes sobre como seus dados serão usados

## 10. ANÁLISE E EXPORTAÇÃO DE DADOS

### A. Analytics e Relatórios
O FastSurvey fornece ferramentas de análise e relatórios. Você concorda em:

- Usar os dados de analytics apenas para melhorar suas pesquisas
- Não usar analytics para rastrear indivíduos sem consentimento
- Respeitar a privacidade dos respondentes ao analisar dados
- Não compartilhar relatórios que contenham dados pessoais sem autorização

### B. Exportação de Dados
Ao exportar dados, você concorda em:

- Usar dados exportados apenas para propósitos legítimos
- Não vender ou alugar dados exportados
- Proteger dados exportados adequadamente
- Cumprir leis de proteção de dados aplicáveis

## 11. PRAZO E TERMINAÇÃO

Você tem permissão para usar a Plataforma pelo período de tempo pelo qual você nos pagou as taxas necessárias para usar a Plataforma. Você concorda que podemos imediatamente e permanentemente desligar seu acesso à Plataforma se você violar materialmente qualquer disposição deste Acordo.

## 12. SEM GARANTIAS

OS SERVIÇOS SÃO FORNECIDOS COMO ESTÃO. O FASTSURVEY RENUNCIA A TODAS AS REPRESENTAÇÕES E GARANTIAS IMPLÍCITAS, INCLUINDO, SEM LIMITAÇÃO, QUALQUER GARANTIA IMPLÍCITA DE COMERCIABILIDADE, ADEQUAÇÃO PARA UM PROPÓSITO PARTICULAR E NÃO VIOLAÇÃO DE DIREITOS DE TERCEIROS, AO MÁXIMO EXTENTO PERMITIDO PELA LEI APLICÁVEL.

O FastSurvey não é responsável por qualquer perda ou dano de sua (ou de seus Usuários) falha em cumprir estes Termos. Você é totalmente responsável por todos os Dados postados na sua conta, e por Dados que você coleta de envios de pesquisa, quer você pessoalmente tenha postado, coletado ou recebido os Dados.

## 13. INDENIZAÇÃO

Você concorda que defenderá o FastSurvey contra reivindicações, incluindo mas não se limitando a processos judiciais, trazidos por terceiros contra o FastSurvey decorrentes de suas pesquisas ou seu uso da Plataforma ou seu uso, coleta ou divulgação de Dados; isso inclui mas não se limita a reivindicações que decorrem de sua violação destes Termos ou da lei ou dos direitos legais de outra pessoa ou entidade ou direitos de privacidade ou direitos de propriedade intelectual como direitos autorais, marca registrada ou direitos de patente. Você também concorda em indenizar o FastSurvey contra danos e custos (incluindo honorários advocatícios e custos judiciais razoáveis) concedidos por um tribunal ou outro tribunal a favor do reclamante ou em acordo da reivindicação. Defenderemos e indenizaremos você contra processos judiciais de terceiros decorrentes da violação do FastSurvey da lei aplicável, e reservamos o direito de resolver tais reivindicações fora do tribunal, às nossas custas, sem sua aprovação.

## 14. LIMITAÇÃO DE RESPONSABILIDADE

EXCETO COMO IMPERMISSÍVEL SOB A LEI, EM NENHUM EVENTO A RESPONSABILIDADE DO FASTSURVEY DECORRENTE DE OU RELACIONADA A ESTE ACORDO, QUER EM CONTRATO, DELITO OU SOB QUALQUER OUTRA TEORIA DE RESPONSABILIDADE, EXCEDERÁ NO TOTAL O VALOR TOTAL PAGO POR VOCÊ PELOS SERVIÇOS NOS DOZE (12) MESES IMEDIATAMENTE PRECEDENDO QUANDO A REIVINDICAÇÃO SURGIU.

O FASTSURVEY ACEITA NENHUMA RESPONSABILIDADE PELAS AÇÕES DE QUALQUER TERCEIRO QUE VOCÊ ENGAJE OU TRABALHE PARA ASSISTIR OU AJUDAR VOCÊ COM SEU USO DE NOSSA PLATAFORMA OU PESQUISAS.

"FASTSURVEY" COMO USADO NESTA SEÇÃO 14 REFERE-SE AO FASTSURVEY E TODAS AS SUAS EMPRESAS RELACIONADAS E AFILIADAS.

## 15. LIMITAÇÃO SOBRE TIPOS DE DANOS

EXCETO SE E NA EXTENSÃO PROIBIDA PELA LEI, EM NENHUM EVENTO SEREMOS RESPONSÁVEIS PARA VOCÊ POR QUALQUER OU TODOS OS SEGUINTES TIPOS DE DANOS, QUER DECORRENTES DA LEI COMUM OU POR ESTATUTO: DANOS POR LUCROS PERDIDOS, TEMPO PERDIDO, NEGÓCIO PERDIDO, OU RECEITAS PERDIDAS, REEMBOLSO DE DINHEIROS PAGOS POR VOCÊ A TERCEIROS PARA AJUDAR VOCÊ A USAR OU APRENDER A USAR A PLATAFORMA OU PARA LIDAR COM PROBLEMAS REAIS OU PERCEBIDOS COM A PLATAFORMA, OU QUALQUER FUNCIONALIDADE FORNECIDA EM OU ATRAVÉS DA PLATAFORMA, POR QUALQUER FORMA DE DANOS DE DIREITOS AUTORAIS OU OUTROS DANOS DE PROPRIEDADE INTELECTUAL DECORRENTES DE SEU USO DA PLATAFORMA OU QUALQUER FUNCIONALIDADE FORNECIDA EM OU ATRAVÉS DA PLATAFORMA, OU POR QUALQUER DANOS INDIRETOS, ESPECIAIS, PUNITIVOS, INCIDENTAIS OU CONSEQUENCIAIS DECORRENTES DE TAL USO, MESMO SE VOCÊ NOS ADVISOU DA POSSIBILIDADE DE TAIS DANOS OCORREREM.

AS LIMITAÇÕES ESTABELECIDAS NESTA SEÇÃO 15 APLICAR-SE-ÃO NÃO OBSTANTE A FALHA DO PROPÓSITO ESSENCIAL DE QUALQUER REMÉDIO E INDEPENDENTEMENTE DA TEORIA LEGAL OU EQUITATIVA SOB A QUAL AS REIVINDICAÇÕES SÃO TRAZIDAS.

## 16. OUTRAS DISPOSIÇÕES

### A. Modificações à Plataforma
O FastSurvey reserva o direito de modificar a Plataforma e qualquer funcionalidade fornecida em ou através da Plataforma, e de parar de oferecer tais coisas para seu uso, com ou sem aviso prévio para você. Sempre nos esforçamos para melhorar nossos produtos e serviços, mas não garantimos ou garantimos que qualquer um deles ou qualquer recurso ou funcionalidade específica sempre estará disponível durante o prazo da sua assinatura. O FastSurvey não será responsável para você ou qualquer terceiro por quaisquer tais modificações ou mudanças.

### B. Entrega de Email de Envios
Quando um terceiro envia uma de suas pesquisas, por padrão enviaremos uma notificação por email para o endereço de email que temos em arquivo para você. Devido a problemas com a internet e sua conexão com ela e similares, não podemos garantir seu recebimento real de tais notificações. Recomendamos instalar aplicativos móveis do FastSurvey para iPhone, Apple Watch ou Android, e verificar sua conta diariamente para garantir que você não perca quaisquer tais notificações.

### C. Renúncia de Garantia Adicional
O FastSurvey não faz garantia de que a Plataforma ou qualquer aspecto dela, ou qualquer coisa que você crie ou use em conexão com ela, estará disponível 100% do tempo ou que estarão livres de erros. Você é totalmente responsável por quaisquer problemas, problemas ou danos que você experimentar por causa de um erro ou erro que você faz em conexão com o uso da Plataforma e qualquer aspecto dela.

### D. Outras Comunicações por Email
Ao dar seu endereço de email ao FastSurvey, você concorda em receber ocasionais emails administrativos, anúncios, newsletters, vendas e marketing do FastSurvey. Você pode cancelar a inscrição desses emails clicando no link "cancelar inscrição" no final dos emails.

### E. Marcas Registradas / Uso de Links do FastSurvey
Você não pode usar ou exibir a marca registrada ou logo do FastSurvey sem nossa permissão por escrito. Se você incluir um link para um website do FastSurvey em suas pesquisas: (a) o(s) link(s) não deve(m) sugerir ou de outra forma criar a falsa aparência de que o FastSurvey é afiliado a qualquer pessoa, entidade ou produto, ou sugerir que o FastSurvey de outra forma endossa, patrocina ou é afiliado a qualquer tal coisa; (b) a aparência, posição e outros aspectos de quaisquer links do FastSurvey não podem ser tais a danificar ou diluir a boa vontade associada ao nome e marcas registradas do FastSurvey; (c) todos os links para nossos websites devem "apontar" para a URL do nosso website principal e não para outras páginas dentro do Website; e (d) todos os links para nossos websites, quando clicados de suas pesquisas, não devem exibir o website dentro de um "frame" no website de link, ou qualquer outro website.

### F. Cessão
Você não pode ceder qualquer ou todos os seus direitos ou obrigações sob este Acordo sem o consentimento prévio por escrito do FastSurvey. Se dermos nosso consentimento, você concorda em garantir que o cessionário concorde por escrito com os termos deste Acordo.

### G. Relacionamento das partes; Sem Beneficiários de Terceiros
As partes aqui são entidades independentes. Nada neste Acordo ou qualquer anexo aqui cria ou criará qualquer parceria, joint venture, agência, franquia ou relacionamento de emprego entre as partes. Não há beneficiários de terceiros para este Acordo.

### H. Escolha da Lei
Este Acordo será regido e interpretado de acordo com as leis do Brasil, excluindo suas disposições de conflito de leis. As partes concordam que a Convenção das Nações Unidas sobre Contratos para a Venda Internacional de Mercadorias não se aplicará a este Acordo.

### I. Disputas / Arbitragem
As partes concordam que todas as disputas entre elas serão finalmente resolvidas por arbitragem vinculante antes de um único árbitro neutro sob os auspícios da Câmara de Arbitragem do Brasil, ou em outro local se ordenado pela câmara ou um tribunal de jurisdição competente. Cada parte terá direito a tomar uma deposição da outra parte, conduzida em um único dia, durando não mais de oito horas. Cada parte terá direito a propor um conjunto de demandas de documentos para a outra parte, consistindo de não mais de dez categorias de documentos, sem sub-partes. Nenhuma outra forma de descoberta será permitida. O árbitro dará uma opinião por escrito declarando a base factual e raciocínio legal para sua decisão. Um prêmio de arbitragem será executável em um tribunal de jurisdição competente.

### J. Período de Limitações
As partes concordam que nenhuma reivindicação será iniciada ou arquivada contra a outra parte mais de um ano após a causa da ação surgir.

### K. Maneira de Dar Aviso
Avisos relativos a este Acordo devem ser por escrito e endereçados a nós para legal@fastsurvey.com.

### L. Força Maior
O FastSurvey não será responsável para você por qualquer atraso ou falha em executar aqui (excluindo obrigações de pagamento que podem ser atrasadas mas não desculpadas) devido a circunstâncias fora do controle razoável da parte, incluindo atos de Deus, atos de governo, pandemia, enchente, fogo, terremotos, agitação civil, atos de terror, greves trabalhistas, interrupções de serviço envolvendo hardware, software ou sistemas de energia não dentro do controle razoável da parte, e ataques de negação de serviço.

### M. Acordo Inteiro
Este Acordo, junto com os Anexos aqui, representa o acordo inteiro das partes concernindo o assunto dele e é intencionado ser a expressão final do acordo e intenção das partes. Este Acordo substitui todos os acordos, propostas e representações prévios e contemporâneos, quer escritos ou orais. As partes concordam que quaisquer termos ou condições declarados ou referenciados em ou em um documento ou documentos outros que este Acordo que contradigam este Acordo são nulos e vazios. Nenhuma emenda, adendo ou outro documento a intenção do qual é adicionar ou de outra forma modificar o Acordo, ou renúncia de qualquer disposição do Acordo, será efetiva a menos que por escrito e assinado por ambas as partes.

### N. Severabilidade; Interpretação; Contrapartes
Se qualquer disposição deste Acordo for mantida por um tribunal de jurisdição competente como contrária à lei, tal disposição será modificada pelo tribunal e interpretada de forma a melhor realizar os objetivos da disposição original na máxima extensão permitida pela lei, e as disposições restantes permanecerão em efeito. As partes expressamente concordam que este Acordo não será interpretado contra qualquer parte como o redator. Este Acordo pode ser executado em contrapartes.

### O. Efeito da Terminação
Após a expiração da sua assinatura, ou terminação deste Acordo por qualquer razão, você concorda em cessar todo acesso e uso da Plataforma FastSurvey. Qualquer terminação não afetará suas obrigações para conosco sob este Acordo (incluindo, sem limitação, pagamentos, propriedade, indenização e limitação de responsabilidade) que são intencionados a sobreviver tal suspensão ou terminação. Teremos direito a descontinuar o hosting de suas pesquisas e Dados, e deletar Dados conforme nossas políticas internas.

### P. Modificações aos Termos
O FastSurvey pode, em seu critério exclusivo e absoluto, modificar estes Termos de tempos em tempos. Se você objetar a quaisquer tais mudanças, seu único recurso será cessar de usar a Plataforma FastSurvey. Uso continuado da Plataforma FastSurvey seguindo aviso de quaisquer tais mudanças indicará seu reconhecimento de tais mudanças e acordo em estar vinculado pelos termos e condições de tais mudanças.

### Q. Links
Você reconhece, entende e concorda que o FastSurvey não endossa ou assume qualquer responsabilidade por qualquer website, produto ou serviço de terceiros que mencionamos ou linkamos em nosso website ou em conexão com a Plataforma.

### R. Entidade Contratante / Jurisdição do Tribunal
Para usuários localizados no Brasil, a entidade contratante do FastSurvey é FastSurvey Ltda., localizada no Brasil, e as leis do Brasil se aplicam a você.

### S. Sem Renúncia / Cabeçalhos / Sobrevivência dos Termos
A falha do FastSurvey em exercer ou fazer cumprir qualquer direito ou disposição destes Termos não constituirá uma renúncia de tal direito ou disposição. Se qualquer disposição destes Termos for encontrada por um tribunal de jurisdição competente como inválida, você ainda assim concorda que o tribunal deve se esforçar para dar efeito às intenções do FastSurvey e você como refletido na disposição, e que as outras disposições destes Termos permanecem em pleno vigor e efeito. Os títulos das seções nestes Termos são apenas para conveniência e não têm efeito legal ou contratual.

Seções 3, 4, e 6-16 destes Termos sobreviverão e continuarão a se aplicar não obstante uma terminação ou expiração da sua assinatura ou seu acesso ou uso da Plataforma.

### T. Vendedores de Terceiros
Se, em seu uso da Plataforma ou qualquer aspecto dela, você habilitar ou usar serviços ou funcionalidade de terceiros não afiliados ao FastSurvey, como integrações, você está dando seu consentimento à Plataforma e ao provedor de terceiros para executar todas as ações consideradas necessárias para o desempenho do serviço incluindo mas não exclusivo a implementar serviços de integração de vendedor em um ambiente de produção ao vivo sem qualquer autorização adicional por você. O FastSurvey não faz garantias ou representações de qualquer tipo com relação a quaisquer produtos, serviços, funcionalidade ou mercadoria fornecidos por Vendedores de Terceiros.

### U. Privacidade / Rastreamento
Ao visitar o Website e/ou usar a Plataforma, você reconhece que, como declarado em nossa Política de Privacidade, usamos cookies de rastreamento e tecnologias similares para entender melhor o comportamento de visitantes e usuários, e você renuncia a quaisquer e todas as reivindicações contra nós decorrentes de nosso uso dessas tecnologias.

## 17. PERGUNTAS

Se você tiver quaisquer perguntas sobre estes termos de uso, sinta-se livre para nos contatar. Para consultas empresariais, planos personalizados, ou quaisquer necessidades específicas além das ofertas padrão, entre em contato com o FastSurvey Enterprise.

**Email:** legal@fastsurvey.com  
**Website:** www.fastsurvey.com  
**Suporte:** suporte@fastsurvey.com

---

**Última atualização:** Dezembro 2024  
**Versão:** 1.0  
**Aplicável a:** Todos os usuários da plataforma FastSurvey
`;

export default function Login() {
  const navigate = useNavigate();

  const [isActive, setIsActive] = useState(false);

  // Login
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [loadingLogin, setLoadingLogin] = useState(false);
  const [showPassLogin, setShowPassLogin] = useState(false);
  const [lembrarDeMim, setLembrarDeMim] = useState(false);

  // Cadastro
  const [novoUsuario, setNovoUsuario] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [novoEmail, setNovoEmail] = useState("");
  const [loadingCadastro, setLoadingCadastro] = useState(false);
  const [showPassCadastro, setShowPassCadastro] = useState(false);
  
  // Validação de usuário
  const [validandoUsuario, setValidandoUsuario] = useState(false);
  const [usuarioDisponivel, setUsuarioDisponivel] = useState(null);
  const [usuarioMensagem, setUsuarioMensagem] = useState("");

  // LGPD
  const [showLgpd, setShowLgpd] = useState(false);
  const [scrolledToEnd, setScrolledToEnd] = useState(false);
  const [accepted, setAccepted] = useState(false);
  const termsRef = useRef(null);
  
  // Estados para confirmação de email
  const [showEmailConfirmation, setShowEmailConfirmation] = useState(false);
  const [confirmationCode, setConfirmationCode] = useState('');
  const [loadingConfirmation, setLoadingConfirmation] = useState(false);
  const [pendingEmail, setPendingEmail] = useState('');

  // Esqueci a senha
  const [showForgot, setShowForgot] = useState(false);
  const [forgotEmail, setForgotEmail] = useState("");
  const [loadingForgot, setLoadingForgot] = useState(false);

  // ====== EFEITOS ======

  // ====== FUNÇÕES ======

  const validarSenhaForte = (s) => /^(?=.*[A-Z])(?=.*\d).{8,}$/.test(s);

  // Validação de nome de usuário em tempo real
  const validarNomeUsuario = async (nome) => {
    if (!nome || nome.length < 3) {
      setUsuarioDisponivel(null);
      setUsuarioMensagem("");
      return;
    }

    setValidandoUsuario(true);
    try {
      const resultado = await checkUsernameAvailability(nome, api);
      setUsuarioDisponivel(resultado.available);
      setUsuarioMensagem(resultado.message);
    } catch (error) {
      console.error('Erro na validação:', error);
      setUsuarioDisponivel(false);
      setUsuarioMensagem("Erro ao verificar disponibilidade");
    } finally {
      setValidandoUsuario(false);
    }
  };

  // Debounce para validação de usuário
  const debounceRef = useRef(null);
  const handleUsuarioChange = (value) => {
    setNovoUsuario(value);
    
    // Limpa o timeout anterior
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }
    
    // Define novo timeout para validar após 500ms
    debounceRef.current = setTimeout(() => {
      validarNomeUsuario(value);
    }, 500);
  };

  // Se já autenticado, redireciona
  useEffect(() => {
    // Limpa tokens inválidos antes de verificar
    let token = localStorage.getItem("token");
    let storageType = 'localStorage';
    
    if (!token) {
      token = sessionStorage.getItem("token");
      storageType = 'sessionStorage';
    }
    
    if (token) {
      try {
        const parts = token.split('.');
        if (parts.length === 3) {
          const payload = parts[1];
          const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
          const parsed = JSON.parse(decoded);
          
          // Se token expirado, limpa o storage correto
          if (parsed?.exp && Date.now() / 1000 > parsed.exp) {
            console.log(`🔍 Token expirado, limpando ${storageType}`);
            if (storageType === 'localStorage') {
              localStorage.removeItem('token');
              localStorage.removeItem('userId');
              localStorage.removeItem('tipousuarioid');
              localStorage.removeItem('loginId');
            } else {
              sessionStorage.removeItem('token');
              sessionStorage.removeItem('userId');
              sessionStorage.removeItem('tipousuarioid');
              sessionStorage.removeItem('loginId');
            }
            return;
          }
        }
      } catch (error) {
        console.log(`🔍 Token inválido, limpando ${storageType}`);
        if (storageType === 'localStorage') {
          localStorage.removeItem('token');
          localStorage.removeItem('userId');
          localStorage.removeItem('tipousuarioid');
          localStorage.removeItem('loginId');
        } else {
          sessionStorage.removeItem('token');
          sessionStorage.removeItem('userId');
          sessionStorage.removeItem('tipousuarioid');
          sessionStorage.removeItem('loginId');
        }
        return;
      }
    }

    // Verificar primeiro localStorage, depois sessionStorage
    let tipo = Number(localStorage.getItem("tipousuarioid"));
    if (!tipo) {
      tipo = Number(sessionStorage.getItem("tipousuarioid"));
    }
    
    if (token && tipo) {
      navigate(tipo === 15 ? "/admin" : "/home", { replace: true });
    }
  }, [navigate]);

  // Garante Authorization no primeiro load (api.js já tem interceptor)
  useEffect(() => {
    // Verificar primeiro localStorage, depois sessionStorage
    let token = localStorage.getItem("token");
    if (!token) {
      token = sessionStorage.getItem("token");
    }
    if (token) api.defaults.headers.common.Authorization = `Bearer ${token}`;
  }, []);

  // Cleanup do debounce quando componente for desmontado
  useEffect(() => {
    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, []);

  async function handleGoogleCredentialResponse(response) {
    // >>> CORRIGIDO: usar idToken (camelCase) como o backend espera
    const idToken = response?.credential;
    if (!idToken) return toast.error("Resposta do Google inválida.");
    
    console.log('🔍 === LOGIN GOOGLE INICIADO ===');
    console.log('idToken recebido:', idToken.substring(0, 50) + '...');
    
    try {
      let data;
      try {
        console.log('🔍 Tentando endpoint preferido:', ENDPOINTS.googleAuthPreferred);
        ({ data } = await api.post(ENDPOINTS.googleAuthPreferred, { idToken }));
      } catch (err) {
        console.log('🔍 Endpoint preferido falhou, tentando fallback:', ENDPOINTS.googleAuthFallback);
        console.log('Erro:', err?.response?.data);
        ({ data } = await api.post(ENDPOINTS.googleAuthFallback, { idToken }));
      }
      
      console.log('🔍 === RESPOSTA DO BACKEND ===');
      console.log('data completa:', data);
      console.log('data.token:', data?.token ? 'Presente' : 'Ausente');
      console.log('data.loginId:', data?.loginId);
      console.log('data.tipoUsuarioId:', data?.tipoUsuarioId);
      console.log('data.usuario:', data?.usuario);
      
      if (!data?.token) return toast.error("Resposta do servidor sem token.");

      // Para login Google, sempre usar localStorage (persistente)
      // Limpar sessionStorage primeiro para evitar conflitos
      sessionStorage.removeItem("token");
      sessionStorage.removeItem("userId");
      sessionStorage.removeItem("tipousuarioid");
      
      localStorage.setItem("token", data.token);
      if (data.loginId != null) localStorage.setItem("userId", String(data.loginId));
      if (data.tipoUsuarioId != null) localStorage.setItem("tipousuarioid", String(data.tipoUsuarioId));
      api.defaults.headers.common.Authorization = `Bearer ${data.token}`;

      console.log('🔍 === DADOS SALVOS NO LOCALSTORAGE ===');
      console.log('token salvo:', localStorage.getItem('token') ? 'Sim' : 'Não');
      console.log('userId salvo:', localStorage.getItem('userId'));
      console.log('tipousuarioid salvo:', localStorage.getItem('tipousuarioid'));

      toast.success(`Olá, ${data.usuario || "usuário Google"}!`);

      const tipo = Number(data.tipoUsuarioId);
      const post = sessionStorage.getItem("postLoginRedirect");
      sessionStorage.removeItem("postLoginRedirect");
      
      console.log('🔍 === REDIRECIONAMENTO GOOGLE ===');
      console.log('Tipo usuário:', tipo);
      console.log('Post login redirect:', post);
      console.log('Vai para admin?', tipo === 15);
      
      setTimeout(() => {
        const target = post && !post.startsWith("/admin") && tipo !== 15 
          ? post 
          : (tipo === 15 ? "/admin" : "/home");
        
        console.log('🔍 Navegando para:', target);
        return navigate(target, { replace: true });
      }, 400);
    } catch (err) {
      console.error('🔍 === ERRO NO LOGIN GOOGLE ===', err);
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Falha na autenticação com Google.";
      toast.error(msg);
    }
  }

  // Login
  const handleLogin = async (e) => {
    e.preventDefault();
    if (!usuario || !senha) return toast.warn("Informe usuário e senha.");
    setLoadingLogin(true);
    try {
      const { data } = await api.post(ENDPOINTS.login, { usuario, senha });
      if (!data?.token) throw new Error("Resposta sem token.");

      // Limpar storage anterior para evitar conflitos
      localStorage.removeItem("token");
      localStorage.removeItem("userId");
      localStorage.removeItem("tipousuarioid");
      sessionStorage.removeItem("token");
      sessionStorage.removeItem("userId");
      sessionStorage.removeItem("tipousuarioid");

      // Salvar token baseado na opção "Lembrar de mim"
      const storage = lembrarDeMim ? localStorage : sessionStorage;
      storage.setItem("token", data.token);
      if (data.loginId != null) storage.setItem("userId", String(data.loginId));
      if (data.tipoUsuarioId != null) storage.setItem("tipousuarioid", String(data.tipoUsuarioId));
      
      // Debug: verificar o que está sendo salvo
      console.log('🔍 === DADOS SALVOS ===');
      console.log('data.loginId:', data.loginId);
      console.log('data.tipoUsuarioId:', data.tipoUsuarioId);
      console.log('storage usado:', lembrarDeMim ? 'localStorage' : 'sessionStorage');
      console.log('tipousuarioid salvo:', storage.getItem('tipousuarioid'));
      api.defaults.headers.common.Authorization = `Bearer ${data.token}`;

      console.log('🔍 === LOGIN BEM-SUCEDIDO ===');
      console.log('Token salvo:', data.token.substring(0, 50) + '...');
      console.log('LoginId salvo:', data.loginId);
      console.log('TipoUsuarioId salvo:', data.tipoUsuarioId);

      toast.success(`Bem-vindo(a), ${data.usuario || usuario}!`);

      const tipo = Number(data.tipoUsuarioId);
      const post = sessionStorage.getItem("postLoginRedirect");
      sessionStorage.removeItem("postLoginRedirect");
      
      console.log('🔍 === REDIRECIONAMENTO ===');
      console.log('Tipo usuário:', tipo);
      console.log('Post login redirect:', post);
      
      setTimeout(() => {
        const target = post && !post.startsWith("/admin") && tipo !== 15 
          ? post 
          : tipo === 15 ? "/admin" : "/home";
        
        console.log('Redirecionando para:', target);
        console.log('URL atual:', window.location.href);
        
        navigate(target, { replace: true });
        
        // Fallback se o navigate não funcionar
        setTimeout(() => {
          if (window.location.pathname !== target) {
            console.log('Navigate falhou, usando window.location');
            window.location.href = target;
          }
        }, 1000);
      }, 100);
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Usuário ou senha inválidos.";
      toast.error(msg);
    } finally {
      setLoadingLogin(false);
    }
  };

  // Cadastro
  const abrirModalLgpd = (e) => {
    e.preventDefault();
    if (!novoUsuario || !novoEmail || !novaSenha)
      return toast.error("Preencha todos os campos de cadastro.");
    if (!validarSenhaForte(novaSenha))
      return toast.error("A senha deve ter no mínimo 8 caracteres, com ao menos uma letra maiúscula e um número.");
    if (usuarioDisponivel === false)
      return toast.error("Nome de usuário já está em uso. Escolha outro nome.");
    if (validandoUsuario)
      return toast.error("Aguarde a validação do nome de usuário.");
    setShowLgpd(true);
    setScrolledToEnd(false);
    setAccepted(false);
  };

  const confirmarCadastro = async () => {
    if (!accepted) return;
    setLoadingCadastro(true);
    try {
      await api.post(ENDPOINTS.register, {
        usuario: novoUsuario,
        email: novoEmail,
        senha: novaSenha
      });
      toast.success("Usuário cadastrado com sucesso! Verifique seu email para confirmar a conta.");
      setPendingEmail(novoEmail);
      setNovoUsuario(""); setNovaSenha(""); setNovoEmail("");
      setShowLgpd(false);
      setShowEmailConfirmation(true);
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Erro ao cadastrar usuário.";
      toast.error(msg);
    } finally {
      setLoadingCadastro(false);
    }
  };

  // Esqueci a senha
  const handleForgot = async () => {
    if (!forgotEmail || !/^\S+@\S+\.\S+$/.test(forgotEmail))
      return toast.warn("Informe um e-mail válido.");
    setLoadingForgot(true);
    try {
      let ok = false, lastErr = null;
      for (const ep of RESET_PASSWORD_ENDPOINTS) {
        try { await api.post(ep, { email: forgotEmail }); ok = true; break; }
        catch (e) { lastErr = e; }
      }
      if (ok) { toast.success("Se o e-mail existir, enviaremos instruções para redefinir a senha."); setShowForgot(false); setForgotEmail(""); }
      else {
        const msg =
          lastErr?.response?.data?.message ||
          (typeof lastErr?.response?.data === "string" ? lastErr.response.data : null) ||
          "Não foi possível solicitar a redefinição de senha.";
        toast.error(msg);
      }
    } finally { setLoadingForgot(false); }
  };

  // Confirmar email com código
  const handleConfirmEmail = async () => {
    if (!confirmationCode || confirmationCode.length !== 6) {
      return toast.warn("Digite o código de 6 dígitos enviado para seu email.");
    }
    
    setLoadingConfirmation(true);
    try {
      await api.post("/api/login/ConfirmarEmail", { token: confirmationCode });
      toast.success("Email confirmado com sucesso! Você já pode fazer login.");
      setShowEmailConfirmation(false);
      setConfirmationCode('');
      setPendingEmail('');
      setTimeout(() => setIsActive(false), 400);
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Erro ao confirmar email.";
      toast.error(msg);
    } finally {
      setLoadingConfirmation(false);
    }
  };

  // Reenviar código de confirmação
  const handleResendConfirmation = async () => {
    if (!pendingEmail || !/^\S+@\S+\.\S+$/.test(pendingEmail))
      return toast.warn("Email inválido.");
    try {
      await api.post("/api/login/ReenviarConfirmacao", { email: pendingEmail });
      toast.success("Código de confirmação reenviado para seu email.");
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Erro ao reenviar código.";
      toast.error(msg);
    }
  };

  // Reenviar verificação (usa o e-mail do CADASTRO)
  const handleResendVerification = async () => {
    if (!novoEmail || !/^\S+@\S+\.\S+$/.test(novoEmail))
      return toast.warn("Informe um e-mail válido no campo de e-mail do cadastro.");
    try {
      let ok = false, lastErr = null;
      for (const ep of RESEND_VERIFICATION_ENDPOINTS) {
        try { await api.post(ep, { email: novoEmail }); ok = true; break; }
        catch (e) { lastErr = e; }
      }
      if (ok) toast.success("Se o e-mail existir, reenviamos o link de verificação.");
      else {
        const msg =
          lastErr?.response?.data?.message ||
          (typeof lastErr?.response?.data === "string" ? lastErr.response.data : null) ||
          "Não foi possível reenviar a verificação.";
        toast.error(msg);
      }
    } catch {
      toast.error("Falha ao reenviar verificação.");
    }
  };

  // Modal LGPD: habilita checkbox ao rolar até o fim
  const onScrollTerms = () => {
    const el = termsRef.current;
    if (!el) return;
    const atEnd = el.scrollTop + el.clientHeight >= el.scrollHeight - 8;
    if (atEnd && !scrolledToEnd) setScrolledToEnd(true);
  };

  useEffect(() => {
    const onKeyDown = (e) => {
      if (e.key === "Escape" && showLgpd && !loadingCadastro) setShowLgpd(false);
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [showLgpd, loadingCadastro]);

  const renderRequisito = (cond, texto) => (
    <p style={{ color: cond ? "green" : "red", fontSize: "12px", margin: "3px 0" }}>
      {cond ? "✓" : "✗"} {texto}
    </p>
  );

  return (
    <div className={styles.viewport}>
      {/* Debug leve de dev (pode remover) */}
      {env.NODE_ENV === 'development' && (
        <div style={{ position: 'absolute', top: 8, left: 8, fontSize: 10, opacity: .6 }}>
          API: {env.REACT_APP_API_URL}
        </div>
      )}

      <div className={`${styles.container} ${isActive ? styles.active : ""}`} id="container">
        {/* ====== CADASTRO ====== */}
        <div className={`${styles["form-container"]} ${styles["sign-up"]}`}>
          <form>
            <h1>Crie sua conta</h1>
            <span>Preencha seus dados para se cadastrar</span>

            <input
              type="text"
              placeholder="Nome de usuário"
              value={novoUsuario}
              onChange={(e) => handleUsuarioChange(e.target.value)}
              required
              autoComplete="username"
            />
            {novoUsuario && (
              <div className={styles["validacao-usuario"]}>
                {validandoUsuario && (
                  <p style={{ color: "orange", fontSize: "12px", margin: "3px 0" }}>
                    ⏳ Verificando disponibilidade...
                  </p>
                )}
                {!validandoUsuario && usuarioDisponivel === true && (
                  <p style={{ color: "green", fontSize: "12px", margin: "3px 0" }}>
                    ✓ {usuarioMensagem}
                  </p>
                )}
                {!validandoUsuario && usuarioDisponivel === false && (
                  <p style={{ color: "red", fontSize: "12px", margin: "3px 0" }}>
                    ✗ {usuarioMensagem}
                  </p>
                )}
              </div>
            )}
            <input
              type="email"
              placeholder="E-mail"
              value={novoEmail}
              onChange={(e) => setNovoEmail(e.target.value)}
              required
              autoComplete="email"
            />

            <div className={styles.inputRow}>
              <input
                type={showPassCadastro ? "text" : "password"}
                placeholder="Senha"
                value={novaSenha}
                onChange={(e) => setNovaSenha(e.target.value)}
                required
                autoComplete="new-password"
                className={styles.inputWithEye}
              />
              <button
                type="button"
                className={styles.eyeBtn}
                onClick={() => setShowPassCadastro((v) => !v)}
                aria-label={showPassCadastro ? "Ocultar senha" : "Mostrar senha"}
                title={showPassCadastro ? "Ocultar senha" : "Mostrar senha"}
              >
                {showPassCadastro ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>

            {novaSenha && (
              <div className={styles["validacao-senha"]}>
                {renderRequisito(novaSenha.length >= 8, "Mínimo de 8 caracteres")}
                {renderRequisito(/[A-Z]/.test(novaSenha), "Pelo menos uma letra maiúscula")}
                {renderRequisito(/\d/.test(novaSenha), "Pelo menos um número")}
              </div>
            )}

            <div className={styles.actionsRow}>
              <button type="button" onClick={abrirModalLgpd} disabled={loadingCadastro}>
                {loadingCadastro ? "Cadastrando..." : "Cadastrar"}
              </button>

              <button
                type="button"
                className={styles.btnLink}
                onClick={handleResendVerification}
                title="Reenviar e-mail de verificação para o e-mail digitado"
              >
                Reenviar verificação
              </button>
            </div>
          </form>
        </div>

        {/* ====== LOGIN ====== */}
        <div className={`${styles["form-container"]} ${styles["sign-in"]}`}>
          <form onSubmit={handleLogin}>
            <h1>FastSurvey</h1>
            <span>Entre com seu usuário e senha</span>

            <input
              type="text"
              placeholder="Usuário"
              value={usuario}
              onChange={(e) => setUsuario(e.target.value)}
              required
              autoComplete="username"
            />

            <div className={styles.inputRow}>
              <input
                type={showPassLogin ? "text" : "password"}
                placeholder="Senha"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                required
                autoComplete="current-password"
                className={styles.inputWithEye}
              />
              <button
                type="button"
                className={styles.eyeBtn}
                onClick={() => setShowPassLogin((v) => !v)}
                aria-label={showPassLogin ? "Ocultar senha" : "Mostrar senha"}
                title={showPassLogin ? "Ocultar senha" : "Mostrar senha"}
              >
                {showPassLogin ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>

            {/* separador */}
            <div className={styles.sep}>
              <div className={styles.sepLine} />
              <span>ou</span>
              <div className={styles.sepLine} />
            </div>

            {/* Google no login */}
            <div className={styles.googleWrap}>
              <GoogleLoginButton
                onSuccess={handleGoogleCredentialResponse}
                onError={() => toast.error("Falha ao autenticar com Google.")}
              />
            </div>

            {/* Lembrar de mim */}
            <div className={styles.rememberMeRow}>
              <label className={styles.checkboxLabel}>
                <input
                  type="checkbox"
                  checked={lembrarDeMim}
                  onChange={(e) => setLembrarDeMim(e.target.checked)}
                  className={styles.checkbox}
                />
                <span className={styles.checkboxText}>Lembrar de mim</span>
              </label>
            </div>

            {/* Esqueci minha senha */}
            <div className={styles.forgotRow}>
              {!showForgot ? (
                <button type="button" className={styles.linkBtn} onClick={() => setShowForgot(true)}>
                  Esqueci minha senha
                </button>
              ) : (
                <div className={styles.forgotInline}>
                  <input
                    type="email"
                    placeholder="Seu e-mail"
                    value={forgotEmail}
                    onChange={(e) => setForgotEmail(e.target.value)}
                  />
                  <button
                    type="button"
                    className={styles.btnMini}
                    onClick={handleForgot}
                    disabled={loadingForgot}
                    title="Enviar link de redefinição"
                  >
                    {loadingForgot ? "Enviando..." : "Enviar link"}
                  </button>
                  <button
                    type="button"
                    className={styles.btnMiniGhost}
                    onClick={() => { setShowForgot(false); setForgotEmail(""); }}
                  >
                    Cancelar
                  </button>
                </div>
              )}
            </div>

            <button type="submit" disabled={loadingLogin}>
              {loadingLogin ? "Entrando..." : "Entrar"}
            </button>
          </form>
        </div>

        {/* Painéis alternáveis */}
        <div className={styles["toggle-container"]}>
          <div className={styles.toggle}>
            <div className={`${styles["toggle-panel"]} ${styles["toggle-left"]}`}>
              <h1>Olá, novo por aqui?</h1>
              <p>Preencha seus dados para começar</p>
              <button
                className={`${styles.toggleBtn} ${styles.btnSolid}`}
                type="button"
                aria-label="Já tenho conta"
                onClick={() => setIsActive(false)}
              >
                Já tenho conta
              </button>
            </div>
            <div className={`${styles["toggle-panel"]} ${styles["toggle-right"]}`}>
              <h1>Bem-vindo de volta!</h1>
              <p>Entre para acessar o sistema</p>
              <button
                className={`${styles.toggleBtn} ${styles.btnOutline}`}
                type="button"
                aria-label="Registrar-se"
                onClick={() => setIsActive(true)}
              >
                Registrar-se
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* ---- MODAL LGPD ---- */}
      {showLgpd && (
        <div className={styles.modalOverlay} role="dialog" aria-modal="true" aria-labelledby="lgpd-title">
          <div className={styles.modalContent}>
            <div className={styles.modalHeader}>
              <h2 id="lgpd-title">Termos de Uso & Privacidade (LGPD)</h2>
              <button
                type="button"
                className={styles.modalClose}
                onClick={() => !loadingCadastro && setShowLgpd(false)}
                aria-label="Fechar"
                disabled={loadingCadastro}
              >
                ×
              </button>
            </div>
            <div
              className={styles.modalBody}
              ref={termsRef}
              onScroll={onScrollTerms}
              tabIndex={0}
            >
              {/* Aviso e texto dos Termos */}
              <p style={{ marginTop: 16, fontStyle: "italic" }}>
                Role até o final para habilitar a opção de aceite.
              </p>
              <div style={{ whiteSpace: "pre-wrap", lineHeight: 1.5, fontSize: 14 }}>
                {TERMS_TEXT}
              </div>
            </div>
            <div className={styles.modalFooter}>
              <label className={styles.checkboxRow}>
                <input
                  type="checkbox"
                  disabled={!scrolledToEnd || loadingCadastro}
                  checked={accepted}
                  onChange={(e) => setAccepted(e.target.checked)}
                />
                <span>Li e aceito os Termos de Uso & Privacidade</span>
              </label>
              <div className={styles.modalActions}>
                <button
                  type="button"
                  className={styles.btnSecondary}
                  onClick={() => setShowLgpd(false)}
                  disabled={loadingCadastro}
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  className={styles.btnPrimary}
                  onClick={confirmarCadastro}
                  disabled={!accepted || loadingCadastro}
                >
                  {loadingCadastro ? "Registrando…" : "Confirmar cadastro"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ---- MODAL CONFIRMAÇÃO DE EMAIL ---- */}
      {showEmailConfirmation && (
        <div className={styles.modalOverlay} role="dialog" aria-modal="true" aria-labelledby="email-confirmation-title">
          <div className={styles.modalContent}>
            <div className={styles.modalHeader}>
              <h2 id="email-confirmation-title">Confirmar Email</h2>
              <button
                type="button"
                className={styles.modalClose}
                onClick={() => !loadingConfirmation && setShowEmailConfirmation(false)}
                aria-label="Fechar"
                disabled={loadingConfirmation}
              >
                ×
              </button>
            </div>
            <div className={styles.modalBody}>
              <p>Enviamos um código de confirmação de 6 dígitos para:</p>
              <p style={{ fontWeight: 'bold', color: '#007bff' }}>{pendingEmail}</p>
              <p>Digite o código abaixo para confirmar sua conta:</p>
              
              <div style={{ marginTop: '20px' }}>
                <input
                  type="text"
                  placeholder="000000"
                  value={confirmationCode}
                  onChange={(e) => {
                    const value = e.target.value.replace(/\D/g, '').slice(0, 6);
                    setConfirmationCode(value);
                  }}
                  style={{
                    width: '100%',
                    padding: '12px',
                    fontSize: '18px',
                    textAlign: 'center',
                    letterSpacing: '4px',
                    border: '2px solid #ddd',
                    borderRadius: '8px',
                    fontFamily: 'monospace'
                  }}
                  maxLength={6}
                  disabled={loadingConfirmation}
                />
              </div>
              
              <div style={{ marginTop: '15px', textAlign: 'center' }}>
                <button
                  type="button"
                  onClick={handleResendConfirmation}
                  style={{
                    background: 'none',
                    border: 'none',
                    color: '#007bff',
                    textDecoration: 'underline',
                    cursor: 'pointer',
                    fontSize: '14px'
                  }}
                  disabled={loadingConfirmation}
                >
                  Não recebeu o código? Reenviar
                </button>
              </div>
            </div>
            <div className={styles.modalFooter}>
              <div className={styles.modalActions}>
                <button
                  type="button"
                  className={styles.btnSecondary}
                  onClick={() => setShowEmailConfirmation(false)}
                  disabled={loadingConfirmation}
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  className={styles.btnPrimary}
                  onClick={handleConfirmEmail}
                  disabled={confirmationCode.length !== 6 || loadingConfirmation}
                >
                  {loadingConfirmation ? "Confirmando…" : "Confirmar Email"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <ToastContainer position="top-right" autoClose={3000} />
    </div>
  );
}
