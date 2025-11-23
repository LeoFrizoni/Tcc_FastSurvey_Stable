import CabecalhoPesquisa from '../../components/layouts/CabecalhoPesquisa';
import InformacoesPesquisa from '../../components/layouts/InformacoesPesquisa';

import React, { useEffect, useState, useCallback, useRef, useMemo } from 'react';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { useParams } from 'react-router-dom';
import TopNavbar from '../../components/layouts/TopNavBar';
import styles from './responderPesquisa.module.css';
import '../../components/layouts/global.css';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import api from '../../lib/api';

/* ================== Config API / Auth ================== */
const API_BASE = 'http://localhost:5062';

// Cache para nomes de usuários
const userNamesCache = new Map();

// Mapeamento hardcoded dos usuários conhecidos (fallback)
const knownUsers = {
  1: 'Leonardo',
  16: 'Rubens',
  35: 'Leonardo Frizoni',
  46: 'aimeebruna',
  49: 'Lucas',
  50: 'Hutao'
};

const gerarSessaoId = () => {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return `sessao-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`;
};

// Função para buscar nome do usuário
async function getUserName(loginId) {
  if (!loginId) return null;

  console.log('Buscando nome do usuário para LoginId:', loginId);

  // Verificar cache primeiro
  if (userNamesCache.has(loginId)) {
    console.log('Nome encontrado no cache:', userNamesCache.get(loginId));
    return userNamesCache.get(loginId);
  }

  try {
    console.log('Tentando buscar via endpoint de admin...');
    // Tentar buscar via endpoint de admin (pode falhar se não for admin)
    const response = await api.get(`/api/admin/usuarios/${loginId}`);
    console.log('Resposta da API admin:', response.data);
    const userName = response.data?.usuario || response.data?.Usuario;
    console.log('Nome extraído:', userName);
    if (userName) {
      userNamesCache.set(loginId, userName);
      console.log('Nome armazenado no cache:', userName);
      return userName;
    }
  } catch (error) {
    console.log('Erro ao buscar nome do usuário via admin:', error.response?.status, error.response?.data);

    // Se falhar, tentar endpoint de perfil (pode funcionar se for o próprio usuário)
    if (error.response?.status === 403 || error.response?.status === 401) {
      try {
        console.log('Tentando endpoint de perfil...');
        const perfilResponse = await api.get('/api/Login/Perfil');
        console.log('Resposta do perfil:', perfilResponse.data);
        const perfilUserName = perfilResponse.data?.usuario || perfilResponse.data?.Usuario;
        if (perfilUserName && perfilResponse.data?.loginId === loginId) {
          userNamesCache.set(loginId, perfilUserName);
          console.log('Nome encontrado via perfil:', perfilUserName);
          return perfilUserName;
        }
      } catch (perfilError) {
        console.log('Erro ao buscar perfil:', perfilError.response?.status);
      }
    }
  }

  // Se não conseguir buscar, tentar mapeamento hardcoded
  if (knownUsers[loginId]) {
    console.log('Usando mapeamento hardcoded:', knownUsers[loginId]);
    userNamesCache.set(loginId, knownUsers[loginId]);
    return knownUsers[loginId];
  }

  console.log('Não foi possível buscar nome do usuário');
  return null;
}

/* =============== Helpers de estilo/layout =============== */
const normalizeTipo = (tipo) =>
  String(tipo || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();

function getContrastingTextColor(bgColor) {
  if (!bgColor) return '#222222';
  let c = String(bgColor).trim();
  if (c.startsWith('var(') || c === 'transparent' || c.startsWith('rgba') || c.startsWith('rgb')) return '#222222';
  c = c.replace('#', '');
  if (c.length === 3) c = c.split('').map(ch => ch + ch).join('');
  if (!/^([0-9a-f]{6})$/i.test(c)) return '#222222';
  const r = parseInt(c.slice(0, 2), 16);
  const g = parseInt(c.slice(2, 4), 16);
  const b = parseInt(c.slice(4, 6), 16);
  const yiq = (r * 299 + g * 587 + b * 114) / 1000;
  return yiq >= 140 ? '#222222' : '#ffffff';
}

function buildBlocoInlineStyle(estilo) {
  const { corFundo, corTexto, fonte } = estilo || {};
  const computedText = corTexto || getContrastingTextColor(corFundo);
  return {
    style: {
      backgroundColor: corFundo || 'var(--paper)',
      color: computedText,
      fontFamily: fonte || 'inherit',
      border: '1px solid var(--border)',
      borderRadius: '12px',
      boxShadow: '0 1px 2px rgba(16,24,40,.04)',
      padding: '14px 16px',
      transition: 'border-color .12s, box-shadow .12s'
    }
  };
}

function resolveImgSrcFromJSON(img){
  if (!img) return null;
  const guess = img.previewUrl || img.url || (img.nome ? `${API_BASE}/Uploads/${img.nome}${img.extensao || ''}` : null);
  return guess || null;
}

function getTipoPesquisaLabel(p){
  console.log('=== DEBUG TIPO PESQUISA ===');
  console.log('Objeto pesquisa completo:', p);
  console.log('tipoPesquisa:', p?.tipoPesquisa);
  console.log('TipoPesquisa:', p?.TipoPesquisa);
  console.log('tipoPesquisaId:', p?.tipoPesquisaId);
  console.log('tipoPesquisaID:', p?.tipoPesquisaID);
  console.log('tipopesquisaid:', p?.tipopesquisaid);

  const direct = p?.tipoPesquisa?.descricao || p?.TipoPesquisa?.descricao;
  if (direct) {
    console.log('Tipo encontrado via descrição direta:', direct);
    return direct;
  }

  const id = p?.tipoPesquisaId ?? p?.tipoPesquisaID ?? p?.tipopesquisaid;
  console.log('ID final extraído:', id, 'Tipo:', typeof id);

  if (id === 1 || id === '1') {
    console.log('Retornando: Pesquisa de Campo');
    return 'Pesquisa de Campo';
  }
  if (id === 2 || id === '2') {
    console.log('Retornando: Teste');
    return 'Teste';
  }

  console.log('Retornando: — (não encontrado)');
  return '—';
}

/* =================== UI menores =================== */
function ActionsPanel({
  onEnviar,
  onExportar,
  disableActions,
  totalRespostas,
  participanteNome,
  onParticipanteNomeChange,
  onRegistrarParticipante,
  registrandoParticipante,
  participanteInfo,
  sessaoId,
  onTrocarParticipante,
}) {
  const participanteRegistrado = Boolean(
    participanteInfo?.participanteId ?? participanteInfo?.ParticipanteId
  );
  const sessaoDisplay = sessaoId || participanteInfo?.sessaoId || participanteInfo?.SessaoId || '—';

  return (
    <aside className={`${styles.actionsPanel} ${styles.noPrint}`} aria-label="Ações">
      <div className={styles.participanteCard}>
        <div className={styles.participanteHeader}>
          <div>
            <h4>Identificação</h4>
            <p>Registre seu nome para gerar a sessão da prova.</p>
          </div>
          {participanteRegistrado && <span className={styles.participanteBadge}>Ativo</span>}
        </div>
        <label htmlFor="participante-nome" className={styles.participanteLabel}>
          Nome do participante
        </label>
        <input
          id="participante-nome"
          type="text"
          value={participanteNome}
          onChange={e => onParticipanteNomeChange(e.target.value)}
          placeholder="Ex.: Maria dos Santos"
          className={styles.participanteInput}
          disabled={registrandoParticipante}
        />
        <button
          type="button"
          className={styles.participanteBtn}
          onClick={onRegistrarParticipante}
          disabled={registrandoParticipante || !participanteNome.trim()}
        >
          {registrandoParticipante
            ? 'Registrando...'
            : participanteRegistrado
              ? 'Atualizar entrada'
              : 'Registrar entrada'}
        </button>
        <div className={styles.participanteMeta}>
          <span>
            Sessão:
            <code>{sessaoDisplay}</code>
          </span>
        </div>
        {participanteRegistrado && (
          <button
            type="button"
            className={styles.participanteLink}
            onClick={onTrocarParticipante}
          >
            Trocar participante
          </button>
        )}
      </div>
      <div className={styles.actionsHeader}>
        <h3>Ações</h3>
        <p className={styles.actionsHint}>
          {totalRespostas > 0 ? `${totalRespostas} resposta(s) preenchida(s)` : 'Nenhuma resposta ainda'}
        </p>
      </div>
      <button className={styles.primaryBtn} onClick={onEnviar} disabled={disableActions}>
        Enviar respostas
      </button>
      <button className={styles.secondaryBtn} onClick={onExportar} disabled={disableActions}>
        Exportar PDF
      </button>
      <div className={styles.smallInfo}>O PDF respeita o layout e quebra em múltiplas páginas.</div>
    </aside>
  );
}

function GaleriaPergunta({ imagensJSON, anexosImg }) {
  const itens = [
    ...(Array.isArray(imagensJSON)
      ? imagensJSON.map((img, i) => ({
          src: resolveImgSrcFromJSON(img),
          alt: img?.nome || img?.file?.name || `imagem-${i}`,
        }))
      : []),
    ...(Array.isArray(anexosImg) ? anexosImg : []),
  ].filter(it => !!it.src);

  if (itens.length === 0) return null;

  return (
    <div className={styles.previewRow}>
      {itens.map((it, i) => (
        <div key={i} className={styles.thumb}>
          <img src={it.src} alt={it.alt} loading="lazy" />
        </div>
      ))}
    </div>
  );
}

/* =================== Página principal =================== */
const ResponderPesquisa = () => {
  const { id: rawId } = useParams();
  const id = rawId?.replace(/[^0-9]/g, ''); // Remove caracteres não numéricos
  const [pesquisa, setPesquisa] = useState(null);
  const [anexos, setAnexos] = useState([]);
  const [respostas, setRespostas] = useState({});
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [autorNome, setAutorNome] = useState(null);
  const [perguntasReais, setPerguntasReais] = useState([]);
  const [sessaoId, setSessaoId] = useState(() => gerarSessaoId());
  const [participanteInfo, setParticipanteInfo] = useState(null);
  const [participanteNome, setParticipanteNome] = useState('');
  const [registrandoParticipante, setRegistrandoParticipante] = useState(false);

  const participanteStorageKey = useMemo(
    () => (id ? `fastsurvey_participante_${id}` : null),
    [id]
  );

  const pdfRef = useRef(null);

  useEffect(() => {
    if (!document.getElementById('hide-pdf-style')) {
      const style = document.createElement('style');
      style.id = 'hide-pdf-style';
      style.innerHTML = `.hide-pdf { display: none !important; }`;
      document.head.appendChild(style);
    }
  }, []);

  useEffect(() => {
    async function carregar() {
      if (!id || !/^\d+$/.test(id)) {
        setErro('ID da pesquisa inválido.');
        setCarregando(false);
        return;
      }

      // Verificar se o ID é válido (maior que 0)
      const idNum = parseInt(id, 10);
      if (idNum <= 0) {
        setErro('ID da pesquisa inválido.');
        setCarregando(false);
        return;
      }

      setCarregando(true);
      setErro('');
      try {
        console.log('Tentando carregar pesquisa com ID:', id);
        console.log('URL completa:', `${api.defaults.baseURL}/api/pesquisas/${id}`);
        const { data } = await api.get(`/api/pesquisas/${id}`);
        console.log('Dados da pesquisa carregados:', data);
        console.log('Estrutura completa da pesquisa:', JSON.stringify(data, null, 2));
        
        const blocos = data.templateJson ? JSON.parse(data.templateJson) : [];
        console.log('=== BLOCOS PARSEADOS ===');
        console.log('Total de blocos:', blocos.length);
        blocos.forEach((b, i) => {
          console.log(`Bloco ${i}:`, {
            perguntaid: b.perguntaid,
            id: b.id,
            perguntaId: b.perguntaId,
            tipo: b.tipo,
            texto: b.texto?.substring(0, 30)
          });
        });

        console.log('✅ BLOCOS PARSEADOS CONCLUÍDO - PRÓXIMO PASSO: BUSCAR PERGUNTAS');
        console.log('🚀 INICIANDO BUSCA DE PERGUNTAS REAIS...');
        console.log('🚀 ID da pesquisa:', id);
        console.log('🚀 URL que será chamada:', `/api/Perguntas/por-pesquisa/${id}`);
        
        // Buscar perguntas reais do backend
        try {
          console.log('🔍 Buscando perguntas reais do backend para pesquisa ID:', id);
          const response = await api.get(`/api/Perguntas/por-pesquisa/${id}`);
          console.log('📦 Resposta completa da API:', response);
          console.log('📦 Data recebida:', response.data);
          
          let perguntas = [];
          const perguntasData = response.data;
          
          // O backend retorna ServiceResult<List<PerguntaResponse>>
          // Pode ser: { success: true, data: [...] } ou apenas [...]
          if (perguntasData?.success && Array.isArray(perguntasData.data)) {
            perguntas = perguntasData.data;
            console.log('✅ Formato ServiceResult detectado');
          } else if (Array.isArray(perguntasData)) {
            perguntas = perguntasData;
            console.log('✅ Formato Array direto detectado');
          } else if (perguntasData?.items && Array.isArray(perguntasData.items)) {
            perguntas = perguntasData.items;
            console.log('✅ Formato PagedResult detectado');
          } else {
            console.warn('⚠️ Formato inesperado:', perguntasData);
          }
          
          console.log(`📋 Total de perguntas extraídas: ${perguntas.length}`);
          console.log('📋 Perguntas:', perguntas);
          
          if (perguntas.length > 0) {
            setPerguntasReais(perguntas);
            
            // Mapear perguntas reais aos blocos pelo índice
            const blocosComIds = blocos.map((bloco, index) => {
              const perguntaReal = perguntas[index];
              console.log(`🔗 Mapeando bloco ${index}:`, {
                textoBloco: bloco.texto?.substring(0, 30),
                perguntaReal: perguntaReal,
                perguntaId: perguntaReal?.perguntaId || perguntaReal?.PerguntaId,
                opcoesReais: perguntaReal?.opcoes
              });
              
              const idReal = perguntaReal?.perguntaId || perguntaReal?.PerguntaId;
              if (perguntaReal && idReal) {
                console.log(`✅ Bloco ${index} mapeado para perguntaId: ${idReal}`);
                
                // Mapear opções reais se existirem
                let opcoesAtualizadas = bloco.opcoes;
                if (perguntaReal.opcoes && Array.isArray(perguntaReal.opcoes) && perguntaReal.opcoes.length > 0) {
                  console.log(`🔢 Mapeando ${perguntaReal.opcoes.length} opções reais para bloco ${index}`);
                  opcoesAtualizadas = bloco.opcoes?.map((opcaoTemplate, opcaoIndex) => {
                    const opcaoReal = perguntaReal.opcoes[opcaoIndex];
                    if (opcaoReal) {
                      console.log(`  ✅ Opção ${opcaoIndex}: opcaoId real = ${opcaoReal.opcaoId}`);
                      return {
                        ...opcaoTemplate,
                        opcaoidReal: opcaoReal.opcaoId, // ID real do banco
                        texto: opcaoReal.texto || opcaoTemplate.texto
                      };
                    }
                    return opcaoTemplate;
                  });
                }
                
                return {
                  ...bloco,
                  perguntaIdReal: idReal,
                  opcoes: opcoesAtualizadas
                };
              }
              console.warn(`⚠️ Bloco ${index} não tem pergunta correspondente`);
              return bloco;
            });
            
            console.log('✅ Blocos com IDs reais mapeados:', blocosComIds);
            setPesquisa({ ...data, blocos: blocosComIds });
          } else {
            console.warn('⚠️ Nenhuma pergunta encontrada no backend');
            setPesquisa({ ...data, blocos });
          }
        } catch (pergErr) {
          console.error('❌❌❌ CAIU NO CATCH! Erro ao buscar perguntas reais:', pergErr);
          console.error('❌ Status HTTP:', pergErr.response?.status);
          console.error('❌ Status Text:', pergErr.response?.statusText);
          console.error('❌ Mensagem:', pergErr.message);
          console.error('❌ Data:', pergErr.response?.data);
          console.error('❌ Config:', pergErr.config);
          console.error('❌ Stack:', pergErr.stack);
          // Se falhar, usar blocos originais
          console.warn('⚠️ Usando blocos SEM IDs reais devido ao erro acima');
          setPesquisa({ ...data, blocos });
        }

        // Buscar nome do autor
        if (data.loginId) {
          console.log('LoginId da pesquisa:', data.loginId);
          const nomeAutor = await getUserName(data.loginId);
          console.log('Nome do autor encontrado:', nomeAutor);
          setAutorNome(nomeAutor);
        }
        
        console.log('✅ Carregamento da pesquisa FINALIZADO');
        console.log('📊 Estado final das perguntas reais (será atualizado em setPerguntasReais)');
      } catch (error) {
        console.error('❌ Erro ao carregar pesquisa:', error);
        if (error.response?.status === 404) {
          setErro('Pesquisa não encontrada.');
        } else if (error.response?.status === 401) {
          setErro('Não autorizado. Faça login para continuar.');
        } else {
          setErro('Não foi possível carregar a pesquisa. Tente novamente mais tarde.');
        }
      } finally {
        setCarregando(false);
      }
    }
    // async function carregarAnexos() {
    //   if (!id || !/^\d+$/.test(id)) {
    //     setAnexos([]);
    //     return;
    //   }

    //   try {
    //     const { data } = await api.get(`/api/Anexos/por-pesquisa/${id}`);
    //     setAnexos(Array.isArray(data) ? data : []);
    //   } catch {
    //     setAnexos([]);
    //   }
    // }
    carregar();
    // carregarAnexos();
  }, [id]);

  useEffect(() => {
    if (!participanteStorageKey || typeof window === 'undefined') return;

    try {
      const storedRaw = localStorage.getItem(participanteStorageKey);
      if (storedRaw) {
        const stored = JSON.parse(storedRaw);
        const sessaoPersistida = stored.sessaoId || stored.SessaoId || gerarSessaoId();
        setSessaoId(sessaoPersistida);
        setParticipanteInfo(stored);
        setParticipanteNome(stored.nomeParticipante || stored.NomeParticipante || '');
        return;
      }
    } catch (error) {
      console.warn('Não foi possível carregar o participante salvo:', error);
    }

    setParticipanteInfo(null);
    setSessaoId(prev => prev || gerarSessaoId());
    setParticipanteNome('');
  }, [participanteStorageKey]);

  const participanteIdAtivo = participanteInfo?.participanteId ?? participanteInfo?.ParticipanteId ?? null;
  const sessaoAtiva = sessaoId || participanteInfo?.sessaoId || participanteInfo?.SessaoId || '';

  const registrarParticipante = useCallback(async () => {
    const nomeLimpo = participanteNome.trim();
    if (!nomeLimpo) {
      toast.warn('Informe seu nome para registrar a sessão.', {
        position: 'top-center',
        autoClose: 3500,
      });
      return;
    }

    const sessaoAtual = sessaoAtiva || gerarSessaoId();
    if (!sessaoAtiva) {
      setSessaoId(sessaoAtual);
    }

    try {
      setRegistrandoParticipante(true);
      const payload = {
        sessaoId: sessaoAtual,
        nomeParticipante: nomeLimpo,
      };

      const pesquisaIdNumero = Number(id);
      if (Number.isInteger(pesquisaIdNumero) && pesquisaIdNumero > 0) {
        payload.pesquisaId = pesquisaIdNumero;
      }

      const { data } = await api.post('/api/ParticipanteSessao/public/entrar', payload);

      const normalizado = {
        participanteId: data?.participanteId ?? data?.ParticipanteId,
        nomeParticipante: data?.nomeParticipante ?? data?.NomeParticipante ?? nomeLimpo,
        sessaoId: data?.sessaoId ?? data?.SessaoId ?? sessaoAtual,
        entrouEm: data?.entrouEm ?? data?.EntrouEm ?? null,
        saiuEm: data?.saiuEm ?? data?.SaiuEm ?? null,
        ativo: data?.ativo ?? data?.Ativo ?? true,
      };

      setParticipanteInfo(normalizado);
      setSessaoId(normalizado.sessaoId);
      if (participanteStorageKey && typeof window !== 'undefined') {
        localStorage.setItem(participanteStorageKey, JSON.stringify(normalizado));
      }

      toast.success('Participante registrado!', {
        position: 'top-center',
        autoClose: 2500,
      });
    } catch (error) {
      console.error('Erro ao registrar participante:', error);
      const mensagem =
        error?.response?.data?.message ||
        error?.response?.data?.error ||
        'Não foi possível registrar a sessão. Tente novamente.';
      toast.error(mensagem, {
        position: 'top-center',
        autoClose: 4000,
      });
    } finally {
      setRegistrandoParticipante(false);
    }
  }, [participanteNome, sessaoAtiva, participanteStorageKey]);

  const limparParticipante = useCallback(() => {
    setParticipanteInfo(null);
    setParticipanteNome('');
    setSessaoId(gerarSessaoId());
    if (participanteStorageKey && typeof window !== 'undefined') {
      localStorage.removeItem(participanteStorageKey);
    }
  }, [participanteStorageKey]);

  const handleParticipanteNomeChange = useCallback((valor) => {
    setParticipanteNome(valor);
  }, []);

  const blocos = useMemo(() => pesquisa?.blocos ?? [], [pesquisa]);

  // Monitor de perguntasReais
  useEffect(() => {
    console.log('🔔 [MONITOR] perguntasReais foi atualizado:', perguntasReais);
    console.log('🔔 [MONITOR] Quantidade:', perguntasReais.length);
    if (perguntasReais.length > 0) {
      console.log('🔔 [MONITOR] Primeira pergunta:', perguntasReais[0]);
    }
  }, [perguntasReais]);

  // Função helper para obter ID da pergunta de forma consistente
  const obterPerguntaId = useCallback((bloco, index) => {
    // PRIORIDADE ABSOLUTA: perguntaIdReal (vem do mapeamento com backend)
    if (bloco.perguntaIdReal) {
      console.log(`🎯 Usando perguntaIdReal para bloco ${index}:`, bloco.perguntaIdReal);
      return bloco.perguntaIdReal;
    }
    
    // SEGUNDA PRIORIDADE: buscar na lista de perguntas reais
    const perguntaReal = perguntasReais[index];
    if (perguntaReal?.perguntaId || perguntaReal?.PerguntaId) {
      const idReal = perguntaReal.perguntaId || perguntaReal.PerguntaId;
      console.log(`🎯 Usando perguntaId da lista de perguntas reais para bloco ${index}:`, idReal);
      return idReal;
    }
    
    // TERCEIRA PRIORIDADE: dados do próprio bloco (se existirem e não forem vazios)
    const candidato = bloco.perguntaid || bloco.id || bloco.perguntaId;
    if (candidato && candidato !== '') {
      console.log(`🎯 Usando ID do bloco para bloco ${index}:`, candidato);
      return candidato;
    }
    
    // ÚLTIMO RECURSO: usar índice temporário (isso NÃO deve acontecer se o backend retornar as perguntas)
    console.warn(`⚠️ AVISO: Usando ID temporário para bloco ${index}. Backend pode não ter retornado perguntas!`);
    return `temp-${index}`;
  }, [perguntasReais]);

  // Mapa: perguntaId -> [{src, alt}]
  // const imagensPorPergunta = useMemo(() => {
  //   const map = new Map();
  //   (anexos || []).forEach(ax => {
  //     const isImg =
  //       (ax.contenttype || '').startsWith('image') ||
  //       /\.(png|jpe?g|gif|webp|bmp|svg)$/i.test(ax.nome || ax.nomeoriginal || '');
  //     if (!isImg) return;
  //     const pid = ax.perguntaid ?? ax.perguntaId;
  //     if (!pid) return;
  //     const arr = map.get(pid) || [];
  //     arr.push({
  //       src: `${API_BASE}/Uploads/${ax.nome}`,
  //       alt: ax.nomeoriginal || ax.nome,
  //     });
  //     map.set(pid, arr);
  //   });
  //   return map;
  // }, [anexos]);

  /* =============== Handlers de resposta =============== */
  const handleCheckboxChange = useCallback((perguntaId, opcaoValor) => {
    setRespostas(prev => {
      const anteriores = Array.isArray(prev[perguntaId]?.valor) ? prev[perguntaId].valor : [];
      const atualizadas = anteriores.includes(opcaoValor)
        ? anteriores.filter(v => v !== opcaoValor)
        : [...anteriores, opcaoValor];
      return { ...prev, [perguntaId]: { tipo: 'multipla', valor: atualizadas } };
    });
  }, []);

  const handleRadioChange = useCallback((perguntaId, opcaoValor) => {
    setRespostas(prev => ({ ...prev, [perguntaId]: { tipo: 'objetiva', valor: opcaoValor } }));
  }, []);

  const handleTextareaChange = useCallback((perguntaId, valor) => {
    setRespostas(prev => ({ ...prev, [perguntaId]: { tipo: 'discursiva', valor: String(valor) } }));
  }, []);

  /* =============== Exportar PDF =============== */
  const exportarPDF = async () => {
    const container = pdfRef.current;
    if (!container) return;

    document.body.classList.add('pdf-export-mode');
    try {
      window.scrollTo(0, 0);
      const canvas = await html2canvas(container, {
        scale: 2,
        useCORS: true,
        backgroundColor: '#ffffff',
        scrollY: -window.scrollY,
        windowWidth: document.documentElement.scrollWidth
      });

      const imgData = canvas.toDataURL('image/png');
      const pdf = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });

      const pageW = pdf.internal.pageSize.getWidth();
      const pageH = pdf.internal.pageSize.getHeight();
      const margin = 10;
      const imgW = pageW - margin * 2;
      const imgH = (canvas.height * imgW) / canvas.width;

      let heightLeft = imgH;
      let position = margin;

      pdf.addImage(imgData, 'PNG', margin, position, imgW, imgH);
      heightLeft -= pageH;

      while (heightLeft > 0) {
        position = margin - (imgH - heightLeft);
        pdf.addPage();
        pdf.addImage(imgData, 'PNG', margin, position, imgW, imgH);
        heightLeft -= pageH;
      }

      const safeTitle = (pesquisa?.titulo || 'resposta-pesquisa')
        .replace(/[\\/:*?"<>|]+/g, '')
        .slice(0, 60);
      pdf.save(`${safeTitle}.pdf`);
      toast.success('PDF exportado com sucesso!', { position: 'top-center', autoClose: 2500 });
    } catch {
      toast.error('Erro ao exportar PDF.', { position: 'top-center', autoClose: 4000 });
    } finally {
      document.body.classList.remove('pdf-export-mode');
    }
  };

  //FIXME ENVIAR RESPOSTAS
  /* =============== Enviar respostas =============== */
  const enviarRespostas = async () => {
    if (!blocos.length) {
      toast.warn('Não há perguntas para enviar.', { position: 'top-center', autoClose: 3000 });
      return;
    }

    // Tentar obter loginId do localStorage, se não existir, será null (resposta anônima)
    const userIdStr = localStorage.getItem('userId');
    const loginId = userIdStr ? parseInt(userIdStr, 10) : null;
    
    console.log('🔐 UserId do localStorage:', userIdStr);
    console.log('🔐 LoginId parseado:', loginId);
    console.log('🔐 Permite respostas anônimas:', pesquisa?.permiteRespostasAnonimas);
    
    // Verificar se precisa estar logado
    if (!loginId && !pesquisa?.permiteRespostasAnonimas) {
      toast.error('Você precisa fazer login para responder esta pesquisa.', { 
        position: 'top-center', 
        autoClose: 4000 
      });
      return;
    }

    if (!sessaoAtiva || !participanteIdAtivo) {
      toast.error('Registre sua entrada antes de enviar as respostas.', {
        position: 'top-center',
        autoClose: 4000,
      });
      return;
    }
    
    const payload = [];

    console.log('=== ENVIAR RESPOSTAS DEBUG ===');
    console.log('Total de blocos:', blocos.length);
    console.log('Blocos completos:', JSON.stringify(blocos, null, 2));
    console.log('Respostas atuais:', respostas);

    blocos.forEach((b, index) => {
      // Usar função helper para obter ID consistente (mesma lógica da renderização)
      const perguntaId = obterPerguntaId(b, index);
      
      console.log(`[ENVIO] Bloco ${index}:`, {
        blocoCompleto: b,
        perguntaIdReal: b.perguntaIdReal,
        perguntaid: b.perguntaid,
        id: b.id,
        perguntaId: b.perguntaId,
        perguntaIdUsado: perguntaId,
        tipo: typeof perguntaId,
        eNumero: !isNaN(Number(perguntaId))
      });
      
      if (perguntaId === null || perguntaId === undefined || perguntaId === '') {
        console.warn(`Bloco ${index} não tem ID válido, pulando...`);
        return;
      }
      
      const r = respostas[perguntaId];
      if (!r) {
        console.log(`Pergunta ${perguntaId} não tem resposta, pulando...`);
        console.log('Respostas disponíveis:', Object.keys(respostas));
        return;
      }
      console.log(`Pergunta ${perguntaId} tem resposta:`, r);

      // Se o ID for temp-*, tentar extrair o ID real
      let perguntaIdNum;
      if (String(perguntaId).startsWith('temp-')) {
        // Buscar o ID real na lista de perguntas
        const perguntaReal = perguntasReais[index];
        const idReal = perguntaReal?.perguntaId || perguntaReal?.PerguntaId;
        
        if (idReal) {
          perguntaIdNum = Number(idReal);
          console.log(`✅ Convertendo ${perguntaId} para ID real: ${perguntaIdNum}`);
        } else {
          console.error(`❌ Não foi possível encontrar ID real para ${perguntaId}`);
          console.error('📋 Perguntas reais disponíveis:', perguntasReais);
          console.error('🔍 Pergunta real no índice:', perguntaReal);
          toast.error(`Erro: Pergunta ${index + 1} não tem ID válido. Recarregue a página.`, { 
            position: 'top-center', 
            autoClose: 5000 
          });
          return;
        }
      } else {
        perguntaIdNum = Number(perguntaId);
      }
      
      if (isNaN(perguntaIdNum) || perguntaIdNum <= 0) {
        console.error(`❌ PerguntaId inválido: ${perguntaId} (convertido: ${perguntaIdNum})`);
        return;
      }

      if (r.tipo === 'discursiva') {
        const texto = String(r.valor || '').trim();
        if (!texto) {
          console.log(`Resposta discursiva vazia para pergunta ${perguntaIdNum}, pulando...`);
          return;
        }
        payload.push({ tipo: 'discursiva', perguntaId: perguntaIdNum, texto });
      } else if (r.tipo === 'objetiva') {
        const val = r.valor;
        if (val == null) {
          console.log(`Resposta objetiva vazia para pergunta ${perguntaIdNum}, pulando...`);
          return;
        }
        payload.push({ tipo: 'objetiva', perguntaId: perguntaIdNum, opcoes: [Number(val)] });
      } else if (r.tipo === 'multipla') {
        const arr = Array.isArray(r.valor) ? r.valor.map(Number) : [];
        if (arr.length === 0) {
          console.log(`Resposta múltipla vazia para pergunta ${perguntaIdNum}, pulando...`);
          return;
        }
        payload.push({ tipo: 'multipla', perguntaId: perguntaIdNum, opcoes: arr });
      }
    });

    //FIXME ERRO
    if (payload.length === 0) {
      toast.info('Preencha ao menos uma resposta antes de enviar.', { position: 'top-center', autoClose: 3200 });
      return;
    }

    try {
      setEnviando(true);
      console.log('=== ENVIANDO RESPOSTAS ===');
      console.log('Payload total:', JSON.stringify(payload, null, 2));
      console.log('LoginId:', loginId, 'PesquisaId:', Number(id));
      
      // Validar PesquisaId
      if (!id || isNaN(Number(id))) {
        throw new Error('PesquisaId inválido.');
      }
      
      // Validar LoginId apenas se a pesquisa NÃO permite respostas anônimas
      if (!pesquisa?.permiteRespostasAnonimas && (!loginId || isNaN(loginId))) {
        throw new Error('Você precisa fazer login para responder esta pesquisa.');
      }
      
      for (const item of payload) {
        // Validação final antes de enviar
        if (!item.perguntaId || item.perguntaId <= 0) {
          console.error('PerguntaId inválido no item:', item);
          continue;
        }

        if (item.tipo === 'discursiva') {
          if (!item.texto || item.texto.trim().length === 0) {
            console.error('Texto vazio para pergunta discursiva:', item);
            continue;
          }
          
          const requestBody = {
            PerguntaId: item.perguntaId,
            Texto: item.texto.trim(),
            LoginId: loginId,
            PesquisaId: Number(id),
            SessaoId: sessaoAtiva,
            RespostaAnonima: !loginId,
            ParticipanteId: participanteIdAtivo,
          };
          console.log('Enviando resposta discursiva:', requestBody);
          await api.post(`/api/Respostas/discursiva`, requestBody);
        } else {
          if (!item.opcoes || item.opcoes.length === 0) {
            console.error('Opções vazias para pergunta objetiva/múltipla:', item);
            continue;
          }
          
          const requestBody = {
            PerguntaId: item.perguntaId,
            OpcoesSelecionadas: item.opcoes,
            LoginId: loginId,
            PesquisaId: Number(id),
            SessaoId: sessaoAtiva,
            RespostaAnonima: !loginId,
            ParticipanteId: participanteIdAtivo,
          };
          console.log('Enviando resposta de opções:', requestBody);
          console.log('Detalhes da requisição:', {
            perguntaId: item.perguntaId,
            tipo: typeof item.perguntaId,
            opcoes: item.opcoes,
            tipoOpcoes: typeof item.opcoes[0],
            loginId: loginId,
            tipoLoginId: typeof loginId
          });
          await api.post(`/api/Respostas/opcoes`, requestBody);
        }
      }
      toast.success('Respostas enviadas com sucesso!', { position: 'top-center', autoClose: 3000 });
      console.log('✅ Todas as respostas enviadas com sucesso!');
    } catch (error) {
      console.error('❌ Erro ao enviar respostas:', {
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
        errors: error.response?.data?.errors,
        title: error.response?.data?.title,
        message: error.message,
        fullError: error
      });
      
      // Mensagem de erro mais específica
      const errorMessage = error.response?.data?.error 
        || error.response?.data?.title 
        || error.message 
        || 'Erro ao enviar respostas. Verifique e tente novamente.';
      
      toast.error(errorMessage, { position: 'top-center', autoClose: 4000 });
    } finally {
      setEnviando(false);
    }
  };

  if (carregando) {
    return (
      <>
        <TopNavbar />
        <div className={styles.page}>
          <div className={styles.grid}>
            <div className={styles.card} aria-busy="true">
              <div className={styles.skeletonTitle} />
              <div className={styles.skeletonText} />
              <div className={styles.skeletonBlock} />
              <div className={styles.skeletonBlock} />
              <div className={styles.skeletonBlock} />
            </div>
            <aside className={styles.actionsPanel}>
              <div className={styles.skeletonBtn} />
              <div className={styles.skeletonBtn} />
            </aside>
          </div>
        </div>
      </>
    );
  }

  if (erro || !pesquisa) {
    return (
      <>
        <TopNavbar />
        <div className={styles.page}>
          <div className={styles.errorBox} role="alert">
            <h2>Ops!</h2>
            <p>{erro || 'Não foi possível carregar esta pesquisa.'}</p>
          </div>
        </div>
      </>
    );
  }

     const autor =
     autorNome ??
     pesquisa?.login?.usuario ??
     pesquisa?.autor?.nome ??
     pesquisa?.autor ??
     pesquisa?.usuario?.nome ??
     (pesquisa?.loginId ? `Usuário ${pesquisa.loginId}` : '—');

  const dataRaw =
    pesquisa?.DataCriacao ??
    pesquisa?.dataCriacao ??
    pesquisa?.dataregistro ??
    pesquisa?.dataRegistro ??
    pesquisa?.createdAt ??
    pesquisa?.criadoEm;

  // Função para formatar data corretamente (DD/MM/YYYY)
  const formatarData = (data) => {
    if (!data) return '—';
    console.log('=== DEBUG DATA ===');
    console.log('Data raw recebida:', data);
    console.log('Tipo da data:', typeof data);

    // Tentar diferentes abordagens para parsear a data
    let date;
    if (typeof data === 'string') {
      // Se for string, tentar parsear diretamente
      date = new Date(data);
    } else {
      date = new Date(data);
    }

    console.log('Objeto Date criado:', date);
    console.log('Date válida?', !isNaN(date.getTime()));

    if (isNaN(date.getTime())) {
      console.log('Data inválida, retornando original');
      return data;
    }

    const dia = String(date.getDate()).padStart(2, '0');
    const mes = String(date.getMonth() + 1).padStart(2, '0');
    const ano = date.getFullYear();

    console.log('Dia:', dia, 'Mês:', mes, 'Ano:', ano);
    const resultado = `${dia}/${mes}/${ano}`;
    console.log('Data formatada final:', resultado);

    // Forçar re-renderização com timestamp único
    const timestamp = Date.now();
    console.log('Timestamp único para data:', timestamp);

    return resultado;
  };

  const dataStr = formatarData(dataRaw);
  console.log('Data da pesquisa:', dataRaw, 'Formatada:', dataStr);
  console.log('Tipo de pesquisa ID da pesquisa:', pesquisa?.tipoPesquisaId);
  console.log('Timestamp atual:', new Date().toISOString());
  console.log('Cache buster:', Math.random());
  console.log('FORÇANDO ATUALIZAÇÃO - VERSÃO 2.0');
  const tipoPesquisaDesc = getTipoPesquisaLabel(pesquisa);

  // Forçar re-renderização com chave única baseada na data
  const renderKey = `${pesquisa?.pesquisaId}-${dataStr}-${Date.now()}-${Math.random()}`;
  console.log('Render key:', renderKey);
  console.log('Data que será exibida:', dataStr);

  return (
    <>
      <div className={styles.wrapper}>
        <TopNavbar />
        <ToastContainer />
        <main className={styles.page}>
          <div className={styles.grid}>
            {/* ===== Coluna esquerda — entra no PDF ===== */}
            <section ref={pdfRef} className={styles.exportArea}>
              <div className={styles.header}>
                <section className={styles.headerSection} aria-label="Cabeçalho da pesquisa">
                  <CabecalhoPesquisa key={`cabecalho-${renderKey}`} autor={autor} data={dataStr} />
                  <InformacoesPesquisa
                    key={`informacoes-${renderKey}`}
                    titulo={pesquisa.titulo}
                    descricao={pesquisa.descricao}
                    tipoPesquisa={tipoPesquisaDesc}
                  />
                </section>
              </div>

              <section className={styles.formArea} aria-label="Formulário da pesquisa">
                {blocos.map((b, index) => {
                  const tipo = normalizeTipo(b.tipo);
                  // Usar função helper para obter ID consistente
                  const perguntaId = obterPerguntaId(b, index);
                  
                  console.log(`[RENDER] Bloco ${index}: perguntaId = ${perguntaId}`, {
                    perguntaIdReal: b.perguntaIdReal,
                    perguntaid: b.perguntaid,
                    tipo: b.tipo
                  });
                  
                  const { style: blocoInline } = buildBlocoInlineStyle(b?.estilo);
                  const resp = respostas[perguntaId];
                  const baseId = `pergunta-${perguntaId}`;
                  return (
                    <div key={perguntaId} className={`${styles.blockWrapper} ${styles.avoidBreak}`}>
                      <div className={styles.block} style={blocoInline}>
                        <div className={styles.blockHeader}>
                          <span className={styles.qIndex} aria-hidden>{index + 1}.</span>
                          <h4 className={styles.qText}>{b.texto}</h4>
                        </div>

                        {/* <GaleriaPergunta
                          imagensJSON={b.imagens}
                          anexosImg={imagensPorPergunta.get(perguntaId)}
                        /> */}

                        {tipo === 'discursiva' && (
                          <div className={styles.field}>
                            <label htmlFor={`${baseId}-txt`} className="sr-only">Resposta</label>
                            <textarea
                              id={`${baseId}-txt`}
                              className={styles.textarea}
                              onChange={e => handleTextareaChange(perguntaId, e.target.value)}
                              placeholder="Resposta do usuário..."
                              rows={4}
                            />
                          </div>
                        )}

                        {tipo === 'objetiva' && Array.isArray(b.opcoes) && (
                          <div className={styles.optionsCol}>
                            {b.opcoes.map((op, idx) => {
                              const isObj = op && typeof op === 'object';
                              // CORREÇÃO: Usar opcaoidReal se disponível (ID do banco), senão usar opcaoid do template
                              const value = isObj && op.opcaoidReal ? Number(op.opcaoidReal) 
                                          : isObj && op.opcaoid ? Number(op.opcaoid) 
                                          : Number(idx);
                              const label = isObj ? (op.texto ?? String(value)) : String(op);
                              const inputId = `${baseId}-opt-${idx}`;
                              const checked = resp?.valor === value;
                              console.log(`Opção objetiva ${idx}: opcaoidReal=${op?.opcaoidReal}, opcaoid=${op?.opcaoid}, value=${value}, label=${label}`);
                              return (
                                <label key={inputId} htmlFor={inputId} className={styles.optionRow}>
                                  <input
                                    id={inputId}
                                    type="radio"
                                    name={baseId}
                                    checked={!!checked}
                                    onChange={() => handleRadioChange(perguntaId, value)}
                                    className={`${styles.radio} ${styles.inputControl}`}
                                  />
                                  <span className={styles.optionLabel}>{label}</span>
                                </label>
                              );
                            })}
                          </div>
                        )}

                        {tipo === 'multipla' && Array.isArray(b.opcoes) && (
                          <div className={styles.optionsCol}>
                            {b.opcoes.map((op, idx) => {
                              const isObj = op && typeof op === 'object';
                              // CORREÇÃO: Usar opcaoidReal se disponível (ID do banco), senão usar opcaoid do template
                              const value = isObj && op.opcaoidReal ? Number(op.opcaoidReal) 
                                          : isObj && op.opcaoid ? Number(op.opcaoid) 
                                          : Number(idx);
                              const label = isObj ? (op.texto ?? String(value)) : String(op);
                              const inputId = `${baseId}-chk-${idx}`;
                              const checked = Array.isArray(resp?.valor) && resp.valor.includes(value);
                              console.log(`Opção múltipla ${idx}: opcaoidReal=${op?.opcaoidReal}, opcaoid=${op?.opcaoid}, value=${value}, label=${label}`);
                              return (
                                <label key={inputId} htmlFor={inputId} className={styles.optionRow}>
                                  <input
                                    id={inputId}
                                    type="checkbox"
                                    checked={!!checked}
                                    onChange={() => handleCheckboxChange(perguntaId, value)}
                                    className={`${styles.checkbox} ${styles.inputControl}`}
                                  />
                                  <span className={styles.optionLabel}>{label}</span>
                                </label>
                              );
                            })}
                          </div>
                        )}

                        {!['discursiva', 'objetiva', 'multipla'].includes(tipo) && (
                          <div className={styles.fieldNote}>(Tipo de bloco não identificado)</div>
                        )}
                      </div>
                    </div>
                  );
                })}
              </section>
            </section>

            {/* ===== Coluna direita — fora do PDF ===== */}
            <ActionsPanel
              onEnviar={enviarRespostas}
              onExportar={exportarPDF}
              disableActions={enviando}
              totalRespostas={Object.keys(respostas).length}
              participanteNome={participanteNome}
              onParticipanteNomeChange={handleParticipanteNomeChange}
              onRegistrarParticipante={registrarParticipante}
              registrandoParticipante={registrandoParticipante}
              participanteInfo={participanteInfo}
              sessaoId={sessaoAtiva}
              onTrocarParticipante={limparParticipante}
            />
          </div>
        </main>
      </div>
    </>
  );
};

export default ResponderPesquisa;
