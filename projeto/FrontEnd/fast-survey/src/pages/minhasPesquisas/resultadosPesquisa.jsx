import CabecalhoPesquisa from '../../components/layouts/CabecalhoPesquisa';
import InformacoesPesquisa from '../../components/layouts/InformacoesPesquisa';

import React, { useEffect, useMemo, useRef, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import TopNavbar from '../../components/layouts/TopNavBar';
import ModalQRCode from '../../components/layouts/ModalQrCode';
import ResultadosCompletos from '../../components/charts/ResultadosCompletos';
import InteractiveSessionModal from '../../components/interactive/InteractiveSessionModal';
import styles from './resultadosPesquisa.module.css';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import api from '../../lib/api';
import { toast } from 'react-toastify';

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

const mapPerguntasDoTemplate = (template) => {
  if (!template) return {};
  const blocos = Array.isArray(template) ? template : [];
  return blocos.reduce((acc, bloco) => {
    const rawId = bloco?.perguntaId ?? bloco?.PerguntaId ?? bloco?.perguntaid ?? bloco?.PerguntaID;
    const perguntaId = rawId != null ? Number(rawId) : null;
    if (!perguntaId || Number.isNaN(perguntaId)) return acc;

    acc[perguntaId] = {
      texto: bloco?.texto ?? bloco?.Texto ?? `Pergunta #${perguntaId}`,
      tipo: (bloco?.tipo ?? bloco?.Tipo ?? '').toString().toLowerCase(),
      opcoes: (bloco?.opcoes ?? bloco?.Opcoes ?? []).reduce((map, opcao, index) => {
        const rawOpcaoId = opcao?.opcaoId ?? opcao?.OpcaoId ?? opcao?.id;
        const opcaoId = rawOpcaoId != null ? Number(rawOpcaoId) : null;
        if (opcaoId && !Number.isNaN(opcaoId)) {
          map[opcaoId] = opcao?.texto ?? opcao?.Texto ?? `Opção ${index + 1}`;
        }
        return map;
      }, {}),
    };

    return acc;
  }, {});
};

const formatarDataHora = (valor) => {
  if (!valor) return '-';
  const data = new Date(valor);
  if (Number.isNaN(data.getTime())) return '-';
  return data.toLocaleString('pt-BR');
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
function normalizeTipo(tipo) {
  return String(tipo || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase();
}

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

function toPx(v, fallback) {
  if (v === 0) return '0px';
  if (v == null) return fallback;
  const s = String(v).trim();
  if (s.endsWith('px') || s.endsWith('%') || s.endsWith('rem')) return s;
  const num = Number(s);
  return Number.isFinite(num) ? `${num}px` : fallback;
}

function buildBlocoInlineStyle(estilo) {
  const {
    corFundo, corTexto, fonte, alinhamento,
    largura, padding, paddingX, paddingY,
    borda, bordaRadius, sombra
  } = estilo || {};

  const computedText = corTexto || getContrastingTextColor(corFundo);

  let finalPadding = padding ?? undefined;
  if (paddingX != null || paddingY != null) {
    const px = toPx(paddingX, '16px');
    const py = toPx(paddingY, '14px');
    finalPadding = `${py} ${px}`;
  }

  return {
    style: {
      backgroundColor: corFundo || 'var(--paper)',
      color: computedText,
      fontFamily: fonte || 'inherit',
      textAlign: alinhamento || 'left',
      maxWidth: largura ? toPx(largura, '100%') : '100%',
      padding: finalPadding || '14px 16px',
      border: borda || '1px solid var(--border)',
      borderRadius: toPx(bordaRadius, '12px'),
      boxShadow: sombra || '0 1px 2px rgba(16,24,40,.04)',
      transition: 'border-color .12s, box-shadow .12s'
    }
  };
}

function resolveImgSrcFromJSON(img){
  if (!img) return null;
  
  // Tentar diferentes formatos de URL
  const possibleUrls = [
    img.previewUrl,
    img.url,
    img.src,
    img.imageUrl,
    img.fileUrl,
    img.nome ? `${API_BASE}/Uploads/${img.nome}${img.extensao || ''}` : null,
    img.nome ? `${API_BASE}/uploads/${img.nome}${img.extensao || ''}` : null,
    img.nome ? `${API_BASE}/Uploads/${img.nome}` : null,
    img.nome ? `${API_BASE}/uploads/${img.nome}` : null,
    // Tentar com diferentes extensões comuns
    img.nome ? `${API_BASE}/Uploads/${img.nome}.jpg` : null,
    img.nome ? `${API_BASE}/Uploads/${img.nome}.jpeg` : null,
    img.nome ? `${API_BASE}/Uploads/${img.nome}.png` : null,
    img.nome ? `${API_BASE}/Uploads/${img.nome}.gif` : null,
    img.nome ? `${API_BASE}/Uploads/${img.nome}.webp` : null,
  ].filter(Boolean);
  
  // Retornar a primeira URL válida
  for (const url of possibleUrls) {
    if (url && typeof url === 'string' && url.trim()) {
      return url;
    }
  }
  
  return null;
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
  
  console.log('Retornando: Indefinido (não encontrado)');
  return 'Indefinido';
}

/* =================== Componentes menores =================== */
function ActionsAside({
  onResponder,
  onQrCode,
  onExportarPDF,
  onEditar,
  onInteractiveSession,
  onVerRespostas,
  carregandoRespostas,
  abaAtiva,
  setAbaAtiva,
  pesquisaId,
  pesquisa,
}) {
  return (
    <aside className={`${styles.actionsPanel} ${styles.noPrint}`} aria-label="Ações">
      {/* Abas */}
      <div className={styles.tabsContainer}>
        <button
          className={`${styles.tabButton} ${abaAtiva === 'pesquisa' ? styles.tabActive : ''}`}
          onClick={() => setAbaAtiva('pesquisa')}
        >
          Pesquisa
        </button>
        <button
          className={`${styles.tabButton} ${abaAtiva === 'graficos' ? styles.tabActive : ''}`}
          onClick={() => setAbaAtiva('graficos')}
        >
          Gráficos
        </button>
      </div>

      {abaAtiva === 'pesquisa' && (
        <>
          <h3 className={styles.actionsTitle}>Ações</h3>
          <button className={styles.primaryBtn} type="button" onClick={onResponder}>
            Responder pesquisa
          </button>
          <button
            className={styles.primaryBtn}
            type="button"
            onClick={onVerRespostas}
            disabled={carregandoRespostas}
          >
            {carregandoRespostas ? 'Carregando respostas…' : 'Ver respostas'}
          </button>
                        {pesquisa?.isInterativa && (
                <button className={styles.interactiveBtn} type="button" onClick={onInteractiveSession}>
                  🎮 Iniciar Sessão Interativa
                </button>
              )}
          <button className={styles.secondaryBtn} type="button" onClick={onQrCode}>
            Mostrar QR Code
          </button>
          <button className={styles.secondaryBtn} type="button" onClick={onExportarPDF}>
            Exportar PDF
          </button>
          <button className={styles.secondaryBtn} type="button" onClick={onEditar}>
            Editar Pesquisa
          </button>
          <p className={styles.smallInfo}>O PDF respeita o layout e quebra em múltiplas páginas.</p>
        </>
      )}

      {abaAtiva === 'graficos' && (
        <ResultadosCompletos pesquisaId={pesquisaId} />
      )}
    </aside>
  );
}

function GaleriaResultados({ imagensJSON, anexosImg }) {
  // Processar imagens do JSON
  const imagensFromJSON = [];
  if (Array.isArray(imagensJSON)) {
    imagensJSON.forEach((img, i) => {
      const src = resolveImgSrcFromJSON(img);
      if (src) {
        imagensFromJSON.push({
          src,
          alt: img?.nome || img?.file?.name || `imagem-${i}`,
        });
      }
    });
  }
  
  // Processar anexos
  const imagensFromAnexos = [];
  if (Array.isArray(anexosImg)) {
    anexosImg.forEach((anexo, i) => {
      if (anexo.src) {
        imagensFromAnexos.push({
          src: anexo.src,
          alt: anexo.alt || `anexo-${i}`,
        });
      }
    });
  }
  
  const itens = [...imagensFromJSON, ...imagensFromAnexos];

  // Se não há itens, retornar null para que o placeholder seja mostrado
  if (itens.length === 0) return null;

  return (
    <div className={styles.previewRow}>
      {itens.map((it, i) => (
        <figure key={i} className={styles.thumb}>
          <img 
            src={it.src} 
            alt={it.alt} 
            loading="lazy"
            onError={(e) => {
              console.log('Erro ao carregar imagem:', it.src);
              e.target.style.display = 'none';
              // Se a imagem falhou, mostrar placeholder no lugar
              const placeholder = document.createElement('div');
              placeholder.className = styles.imagePlaceholder;
              placeholder.innerHTML = `
                <div class="${styles.placeholderContent}">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                    <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                    <circle cx="8.5" cy="8.5" r="1.5"/>
                    <path d="m21 15-3.086-3.086a2 2 0 0 0-2.828 0L6 21"/>
                  </svg>
                  <span>Imagem não disponível</span>
                </div>
              `;
              e.target.parentElement.appendChild(placeholder);
            }}
          />
        </figure>
      ))}
    </div>
  );
}

/* =================== Página principal =================== */
const ResultadosPesquisa = () => {
  const { id: rawId } = useParams();
  const id = rawId?.replace(/[^0-9]/g, ''); // Remove caracteres não numéricos
  const navigate = useNavigate();
  const [pesquisa, setPesquisa] = useState(null);
  const [anexos, setAnexos] = useState([]);
  const [mostrarModalQr, setMostrarModalQr] = useState(false);
  const [mostrarModalInterativa, setMostrarModalInterativa] = useState(false);
  const [carregando, setCarregando] = useState(true);
  const [abaAtiva, setAbaAtiva] = useState('pesquisa'); // 'pesquisa' ou 'graficos'
  const [erro, setErro] = useState('');
  const [autorNome, setAutorNome] = useState(null);
  const pdfRef = useRef(null);
  const [mostrarRespostas, setMostrarRespostas] = useState(false);
  const [respostas, setRespostas] = useState([]);
  const [carregandoRespostas, setCarregandoRespostas] = useState(false);
  const [erroRespostas, setErroRespostas] = useState('');
  const [filtroUsuario, setFiltroUsuario] = useState('todos');
  const [buscaUsuario, setBuscaUsuario] = useState('');
  const [participantesInfo, setParticipantesInfo] = useState({});

  useEffect(() => {
    async function buscarPesquisa() {
      if (!id || !/^\d+$/.test(id)) {
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
        if (data.templateJson) {
          try {
            const blocos = JSON.parse(data.templateJson);
            setPesquisa({ ...data, blocos });
          } catch {
            setPesquisa(data);
          }
        } else {
          setPesquisa(data);
        }
        
        // Buscar nome do autor
        if (data.loginId) {
          console.log('LoginId da pesquisa:', data.loginId);
          const nomeAutor = await getUserName(data.loginId);
          console.log('Nome do autor encontrado:', nomeAutor);
          setAutorNome(nomeAutor);
        }
            } catch (e) {
        console.error('Erro ao carregar pesquisa:', e);
        if (e.response?.status === 401) {
          setErro('Não autorizado. Faça login para continuar.');
          setTimeout(() => navigate('/login'), 1200);
        } else if (e.response?.status === 404) {
          setErro('Pesquisa não encontrada.');
        } else {
          setErro('Não foi possível carregar a pesquisa. Tente novamente mais tarde.');
        }
      } finally {
        setCarregando(false);
      }
    }

        async function carregarAnexos() {
      if (!id || !/^\d+$/.test(id)) {
        setAnexos([]);
        return;
      }
      
      try {
        const { data } = await api.get(`/api/Anexos/por-pesquisa/${id}`);
        setAnexos(Array.isArray(data) ? data : []);
      } catch (error) {
        // Ignorar erro 404 do endpoint de anexos
        console.log('Endpoint de anexos não disponível, continuando sem anexos...');
        setAnexos([]);
      }
    }

    buscarPesquisa();
    carregarAnexos();
  }, [id, navigate]);

  const blocos = useMemo(() => pesquisa?.blocos ?? [], [pesquisa]);
  const perguntasDetalhes = useMemo(() => mapPerguntasDoTemplate(blocos), [blocos]);

  const atualizarParticipantesInfo = useCallback(
    async (lista) => {
      const ids = Array.from(
        new Set(
          lista
            .map((resposta) => resposta?.participanteId ?? resposta?.ParticipanteId)
            .filter((pid) => pid && !participantesInfo[pid])
        )
      );

      if (ids.length === 0) return;

      try {
        const resultados = await Promise.all(
          ids.map(async (pid) => {
            try {
              const { data } = await api.get(`/api/ParticipanteSessao/${pid}`);
              const nome = data?.nome ?? data?.Nome ?? data?.usuario ?? data?.Usuario;
              return { pid, nome: nome || `Participante #${pid}` };
            } catch (error) {
              console.error('Erro ao buscar participante', pid, error);
              return { pid, nome: `Participante #${pid}` };
            }
          })
        );

        setParticipantesInfo((prev) => {
          const next = { ...prev };
          resultados.forEach(({ pid, nome }) => {
            next[pid] = nome;
          });
          return next;
        });
      } catch (error) {
        console.error('Erro ao carregar informações de participantes', error);
      }
    },
    [participantesInfo]
  );

  const carregarRespostas = useCallback(async () => {
    if (!id) return;
    setCarregandoRespostas(true);
    setErroRespostas('');
    try {
      const { data } = await api.get(`/api/respostas/pesquisa/${id}`);
      const lista = Array.isArray(data) ? data : [];
      setRespostas(lista);
      await atualizarParticipantesInfo(lista);
    } catch (error) {
      console.error('Erro ao carregar respostas:', error);
      setErroRespostas('Não foi possível carregar as respostas desta pesquisa.');
      toast.error('Não foi possível carregar as respostas.');
    } finally {
      setCarregandoRespostas(false);
    }
  }, [id, atualizarParticipantesInfo]);

  const abrirModalRespostas = () => {
    setMostrarRespostas(true);
    if (respostas.length === 0) {
      carregarRespostas();
    }
  };

  const fecharModalRespostas = () => {
    setMostrarRespostas(false);
    setFiltroUsuario('todos');
    setBuscaUsuario('');
  };

  const rotuloParticipante = useCallback(
    (participanteId) => {
      if (!participanteId) return 'Anônimo';
      return participantesInfo[participanteId] || `Participante #${participanteId}`;
    },
    [participantesInfo]
  );

  const respostasFiltradas = useMemo(() => {
    const textoBusca = buscaUsuario.trim().toLowerCase();
    return respostas.filter((resposta) => {
      const participanteId = resposta?.participanteId ?? resposta?.ParticipanteId;

      if (filtroUsuario === 'anon' && participanteId) {
        return false;
      }

      if (textoBusca) {
        const label = rotuloParticipante(participanteId).toLowerCase();
        if (!label.includes(textoBusca)) {
          return false;
        }
      }

      return true;
    });
  }, [respostas, filtroUsuario, buscaUsuario, rotuloParticipante]);

  const participantesOptions = useMemo(() => {
    const ids = Array.from(
      new Set(
        respostas
          .map((resposta) => resposta?.participanteId ?? resposta?.ParticipanteId)
          .filter((pid) => pid != null)
      )
    );
    return ids.map((pid) => ({ id: String(pid), label: participantesInfo[pid] || `Participante #${pid}` }));
  }, [respostas, participantesInfo]);

  const renderRespostaConteudo = useCallback(
    (resposta) => {
      const texto = resposta?.texto ?? resposta?.Texto;
      const opcoesSelecionadas = resposta?.opcoes ?? resposta?.Opcoes ?? [];
      const perguntaId = resposta?.perguntaId ?? resposta?.PerguntaId;
      const detalhe = perguntaId ? perguntasDetalhes[Number(perguntaId)] : null;

      if (opcoesSelecionadas.length > 0) {
        return (
          <ul className={styles.responseOptions}>
            {opcoesSelecionadas.map((opcaoId) => (
              <li key={`${perguntaId}-${opcaoId}`}>
                {detalhe?.opcoes?.[opcaoId] ?? `Opção #${opcaoId}`}
              </li>
            ))}
          </ul>
        );
      }

      if (texto) {
        return <p className={styles.responseText}>{texto}</p>;
      }

      return <p className={styles.emptyAnswer}>Resposta sem conteúdo.</p>;
    },
    [perguntasDetalhes]
  );

  // Mapa: perguntaId -> [{src, alt}]
  const imagensPorPergunta = useMemo(() => {
    const map = new Map();
    (anexos || []).forEach(ax => {
      const isImg =
        (ax.contenttype || '').startsWith('image') ||
        /\.(png|jpe?g|gif|webp|bmp|svg)$/i.test(ax.nome || ax.nomeoriginal || '');
      if (!isImg) return;
      const pid = ax.perguntaid ?? ax.perguntaId;
      if (!pid) return;
      const arr = map.get(pid) || [];
      arr.push({
        src: `${API_BASE}/Uploads/${ax.nome}`,
        alt: ax.nomeoriginal || ax.nome,
      });
      map.set(pid, arr);
    });
    return map;
  }, [anexos]);

  async function exportarPDF() {
    const node = pdfRef.current;
    if (!node) return;

    document.body.classList.add('pdf-export-mode');
    try {
      window.scrollTo(0, 0);

      const canvas = await html2canvas(node, {
        scale: 2,
        backgroundColor: '#ffffff',
        useCORS: true,
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

      const safeTitle = (pesquisa?.titulo || 'resultado-pesquisa')
        .replace(/[\\/:*?"<>|]+/g, '')
        .slice(0, 60);
      pdf.save(`${safeTitle}.pdf`);
    } catch {
      alert('Erro ao exportar PDF.');
    } finally {
      document.body.classList.remove('pdf-export-mode');
    }
  }

  if (carregando) {
    return (
      <>
        <TopNavbar />
        <div className={styles.page}>
          <div className={styles.grid}>
            <div className={styles.card} aria-busy="true" aria-live="polite">
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
            <div className={styles.errorActions}>
              <button className={styles.secondaryBtn} onClick={() => navigate('/login')}>Ir para o login</button>
              <button className={styles.secondaryBtn} onClick={() => navigate('/')}>Voltar para Home</button>
            </div>
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
      <TopNavbar />
      <main className={styles.page}>
        <div className={styles.grid}>
          {/* ===== Coluna esquerda — entra no PDF ===== */}
          <section ref={pdfRef} className={styles.exportArea}>
                         <section className={styles.headerSection} aria-label="Cabeçalho da pesquisa">
               <CabecalhoPesquisa key={`cabecalho-${renderKey}`} autor={autor} data={dataStr} />
               <InformacoesPesquisa
                 key={`informacoes-${renderKey}`}
                 titulo={pesquisa.titulo || "Sem título"}
                 descricao={pesquisa.descricao || "Sem descrição"}
                 tipoPesquisa={tipoPesquisaDesc}
               />
             </section>

            <section className={styles.formArea} aria-label="Estrutura da pesquisa">
              {blocos.length === 0 && (
                <div className={styles.emptyBox}>Nenhum bloco cadastrado nesta pesquisa.</div>
              )}
                             {blocos.map((bloco, index) => {
                 const { style: blocoInline } = buildBlocoInlineStyle(bloco?.estilo);
                 const tipo = normalizeTipo(bloco.tipo);
                 const key = bloco.perguntaid ?? bloco.id ?? index;
                 
                 return (
                  <div key={key} className={`${styles.blockWrapper} ${styles.avoidBreak}`}>
                    <article className={styles.block} style={blocoInline}>
                      <div className={styles.blockHeader}>
                        <span className={styles.qIndex} aria-hidden> {index + 1}. </span>
                        <h4 className={styles.qText}>{bloco.texto}</h4>
                      </div>

                                             {/* Verificar se há imagens válidas */}
                       {(() => {
                         const temImagensJSON = Array.isArray(bloco.imagens) && bloco.imagens.length > 0;
                         const temAnexos = imagensPorPergunta.get(key) && imagensPorPergunta.get(key).length > 0;
                         
                         if (temImagensJSON || temAnexos) {
                           return (
                             <GaleriaResultados
                               imagensJSON={bloco.imagens}
                               anexosImg={imagensPorPergunta.get(key)}
                             />
                           );
                         } else {
                           // Mostrar placeholder apenas se não há imagens
                           return (
                             <div className={styles.imagePlaceholder}>
                               <div className={styles.placeholderContent}>
                                 <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
                                   <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                                   <circle cx="8.5" cy="8.5" r="1.5"/>
                                   <path d="m21 15-3.086-3.086a2 2 0 0 0-2.828 0L6 21"/>
                                 </svg>
                                 <span>Imagem não disponível</span>
                               </div>
                             </div>
                           );
                         }
                       })()}
                      
                      {/* Anexos da pergunta */}
                      {(bloco.imagens && bloco.imagens.length > 0) && (
                        <div className={styles.questionAttachments}>
                          <div className={styles.attachmentsTitle}>
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                              <path d="m21.44 11.05-9.19 9.19a6 6 0 0 1-8.49-8.49l8.57-8.57A4 4 0 1 1 18 8.84l-8.59 8.57a2 2 0 0 1-2.83-2.83l8.49-8.48"/>
                            </svg>
                            Anexos da Pergunta
                          </div>
                        </div>
                      )}

                      {tipo === 'discursiva' && (
                        <div className={styles.discursiveContainer}>
                          {bloco.respostaExemplo && (
                            <div className={styles.exampleAnswer}>
                              <label className={styles.exampleLabel}>Exemplo de resposta (para IA no futuro)</label>
                              <div className={styles.exampleText}>{bloco.respostaExemplo}</div>
                            </div>
                          )}
                          <div className={styles.answerBoxMuted} aria-label="Resposta do usuário (discursiva)">
                            Resposta do usuário...
                          </div>
                        </div>
                      )}

                      {(tipo === 'objetiva' || tipo === 'multipla') && Array.isArray(bloco.opcoes) && (
                        <div className={styles.optionsContainer}>
                          {/* Configurações da pergunta */}
                          {(bloco.temGabarito || bloco.permitirMultiplaSelecao) && (
                            <div className={styles.questionConfig}>
                              {bloco.temGabarito && (
                                <span className={styles.configBadge}>Há gabarito</span>
                              )}
                              {bloco.permitirMultiplaSelecao && (
                                <span className={styles.configBadge}>Permitir múltipla seleção</span>
                              )}
                            </div>
                          )}
                          
                          {/* Opções */}
                          <ul className={styles.optionsList}>
                            {bloco.opcoes.map((op, i) => {
                              const label = (op && typeof op === 'object') ? (op.texto ?? String(i + 1)) : String(op);
                              const isCorrect = op && typeof op === 'object' ? op.correta : false;
                              return (
                                <li key={op?.opcaoid ?? i} className={`${styles.optionChip} ${isCorrect ? styles.correctOption : ''}`}>
                                  <span className={styles.optionText}>{label}</span>
                                  {isCorrect && (
                                    <span className={styles.correctBadge}>Correta</span>
                                  )}
                                </li>
                              );
                            })}
                          </ul>
                        </div>
                      )}

                      {!['discursiva', 'objetiva', 'multipla'].includes(tipo) && (
                        <div className={styles.fieldNote}>(Tipo de bloco não identificado)</div>
                      )}
                    </article>
                  </div>
                );
              })}
            </section>
          </section>

          {/* ===== Coluna direita — fora do PDF ===== */}
          <ActionsAside
            onResponder={() => navigate(`/responder/${id}`)}
            onQrCode={() => setMostrarModalQr(true)}
            onExportarPDF={exportarPDF}
            onEditar={() => navigate(`/editarPesquisa/${id}`)}
            onInteractiveSession={() => setMostrarModalInterativa(true)}
            onVerRespostas={abrirModalRespostas}
            carregandoRespostas={carregandoRespostas}
            abaAtiva={abaAtiva}
            setAbaAtiva={setAbaAtiva}
            pesquisaId={id}
            pesquisa={pesquisa}
          />
        </div>

        <ModalQRCode
          isOpen={mostrarModalQr}
          onClose={() => setMostrarModalQr(false)}
          qrUrl={`${window.location.origin}/responder/${id}`}
        />

        <InteractiveSessionModal
          isOpen={mostrarModalInterativa}
          onClose={() => setMostrarModalInterativa(false)}
          pesquisaId={id}
          pesquisaTitulo={pesquisa?.titulo || 'Pesquisa'}
        />

        {mostrarRespostas && (
          <div className={styles.responsesOverlay} onMouseDown={fecharModalRespostas}>
            <div
              className={styles.responsesModal}
              role="dialog"
              aria-modal="true"
              onMouseDown={(e) => e.stopPropagation()}
            >
              <div className={styles.responsesHeader}>
                <div>
                  <h3>Respostas da pesquisa</h3>
                  <p>{respostas.length} recebidas</p>
                </div>
                <button type="button" className={styles.closeButton} onClick={fecharModalRespostas}>
                  Fechar
                </button>
              </div>

              <div className={styles.responsesFilters}>
                <label htmlFor="filtro-usuario-input">Filtrar por usuário</label>
                <div className={styles.filtersRow}>
                  <input
                    id="filtro-usuario-input"
                    type="text"
                    placeholder="Digite o nome do participante"
                    value={buscaUsuario}
                    onChange={(e) => setBuscaUsuario(e.target.value)}
                    list="participantes-suggestions"
                    className={styles.filtersInput}
                  />
                  <datalist id="participantes-suggestions">
                    {participantesOptions.map((opt) => (
                      <option key={opt.id} value={opt.label} />
                    ))}
                  </datalist>
                  <div className={styles.filtersChips}>
                    <button
                      type="button"
                      className={`${styles.chipButton} ${filtroUsuario === 'todos' ? styles.chipButtonActive : ''}`}
                      onClick={() => setFiltroUsuario('todos')}
                    >
                      Todos
                    </button>
                    {respostas.some((resposta) => !(resposta?.participanteId ?? resposta?.ParticipanteId)) && (
                      <button
                        type="button"
                        className={`${styles.chipButton} ${filtroUsuario === 'anon' ? styles.chipButtonActive : ''}`}
                        onClick={() => setFiltroUsuario('anon')}
                      >
                        Anônimas
                      </button>
                    )}
                  </div>
                  <button
                    type="button"
                    className={styles.refreshButton}
                    onClick={carregarRespostas}
                    disabled={carregandoRespostas}
                  >
                    {carregandoRespostas ? 'Atualizando…' : 'Atualizar'}
                  </button>
                </div>
                {buscaUsuario && (
                  <span className={styles.filtersHint}>
                    Mostrando nomes que contenham “{buscaUsuario}”.
                  </span>
                )}
              </div>

              <div className={styles.responsesContent}>
                {carregandoRespostas ? (
                  <div className={styles.responsesLoading}>Carregando respostas…</div>
                ) : erroRespostas ? (
                  <p className={styles.errorMessage}>{erroRespostas}</p>
                ) : respostasFiltradas.length === 0 ? (
                  <p className={styles.emptyAnswer}>Nenhuma resposta encontrada para este filtro.</p>
                ) : (
                  <div className={styles.responsesList}>
                    {respostasFiltradas.map((resposta) => {
                      const respostaId =
                        resposta?.respostaId ??
                        resposta?.RespostaId ??
                        `${resposta?.perguntaId ?? resposta?.PerguntaId}-${resposta?.dataResposta ?? resposta?.DataResposta}`;
                      const participanteId = resposta?.participanteId ?? resposta?.ParticipanteId;
                      const perguntaId = resposta?.perguntaId ?? resposta?.PerguntaId;
                      const perguntaDetalhe = perguntaId ? perguntasDetalhes[Number(perguntaId)] : null;
                      const perguntaTexto = perguntaDetalhe?.texto ?? (perguntaId ? `Pergunta #${perguntaId}` : 'Pergunta');
                      const dataResposta = resposta?.dataResposta ?? resposta?.DataResposta;
                      const ehAnonima = resposta?.respostaAnonima ?? resposta?.RespostaAnonima ?? false;

                      return (
                        <article key={respostaId} className={styles.responseItem}>
                          <div className={styles.responseMeta}>
                            <div>
                              <h4>{perguntaTexto}</h4>
                              <small>{formatarDataHora(dataResposta)}</small>
                            </div>
                            <div className={styles.responseUser}>
                              <span>{rotuloParticipante(participanteId)}</span>
                              {ehAnonima && <span className={styles.responseTag}>Anônima</span>}
                            </div>
                          </div>
                          <div className={styles.responseAnswer}>{renderRespostaConteudo(resposta)}</div>
                        </article>
                      );
                    })}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </main>
    </>
  );
};

export default ResultadosPesquisa;
