import CabecalhoPesquisa from '../../components/layouts/CabecalhoPesquisa';
import InformacoesPesquisa from '../../components/layouts/InformacoesPesquisa';

import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import ModalQRCode from '../../components/layouts/ModalQrCode';
import ResultadosCompletos from '../../components/charts/ResultadosCompletos';
import InteractiveSessionModal from '../../components/game/InteractiveSessionModal';
import styles from './resultadosPesquisa.module.css';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

/* ================== Config API / Auth ================== */
const API = 'http://localhost:5062/api';
const API_BASE = API.replace(/\/api$/,''); // -> http://localhost:5062

function getToken() {
  const keys = ['token', 'authToken', 'accessToken', 'jwt', 'Authorization'];
  for (const k of keys) {
    const v = localStorage.getItem(k) || sessionStorage.getItem(k);
    if (v) return v.replace(/^Bearer\s+/i, '');
  }
  return null;
}

const api = axios.create({ baseURL: API });
api.interceptors.request.use((config) => {
  const tk = getToken();
  if (tk) config.headers.Authorization = `Bearer ${tk}`;
  return config;
});

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
  const direct = p?.tipoPesquisa?.descricao || p?.TipoPesquisa?.descricao;
  if (direct) return direct;
  const id = p?.tipoPesquisaId ?? p?.tipoPesquisaID ?? p?.tipopesquisaid;
  if (id === 1) return 'Pública';
  if (id === 2) return 'Privada';
  return 'Indefinido';
}

/* =================== Componentes menores =================== */
function ActionsAside({ onResponder, onQrCode, onExportarPDF, onEditar, onInteractiveSession, abaAtiva, setAbaAtiva, pesquisaId }) {
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
          <button className={styles.interactiveBtn} type="button" onClick={onInteractiveSession}>
            🎮 Iniciar Sessão Interativa
          </button>
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
  const { id } = useParams();
  const navigate = useNavigate();
  const [pesquisa, setPesquisa] = useState(null);
  const [anexos, setAnexos] = useState([]);
  const [mostrarModalQr, setMostrarModalQr] = useState(false);
  const [mostrarModalInterativa, setMostrarModalInterativa] = useState(false);
  const [carregando, setCarregando] = useState(true);
  const [abaAtiva, setAbaAtiva] = useState('pesquisa'); // 'pesquisa' ou 'graficos'
  const [erro, setErro] = useState('');
  const pdfRef = useRef(null);

  useEffect(() => {
    async function buscarPesquisa() {
      setCarregando(true);
      setErro('');
      try {
        const { data } = await api.get(`/pesquisas/${id}`);
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
      } catch (e) {
        if (axios.isAxiosError(e) && e.response?.status === 401) {
          setErro('Não autorizado. Faça login para continuar.');
          setTimeout(() => navigate('/login'), 1200);
        } else if (axios.isAxiosError(e) && e.response?.status === 404) {
          setErro('Pesquisa não encontrada.');
        } else {
          setErro('Não foi possível carregar a pesquisa. Tente novamente mais tarde.');
        }
      } finally {
        setCarregando(false);
      }
    }

         async function carregarAnexos() {
       try {
         const { data } = await api.get(`/Anexos/pesquisa/${id}`);
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
    pesquisa?.login?.usuario ??
    pesquisa?.autor?.nome ??
    pesquisa?.autor ??
    pesquisa?.usuario?.nome ??
    '—';

  const dataRaw =
    pesquisa?.DataCriacao ??
    pesquisa?.dataCriacao ??
    pesquisa?.dataregistro ??
    pesquisa?.dataRegistro ??
    pesquisa?.createdAt ??
    pesquisa?.criadoEm;

  const dataStr = dataRaw ? new Date(dataRaw).toLocaleDateString('pt-BR') : '—';
  const tipoPesquisaDesc = getTipoPesquisaLabel(pesquisa);



  return (
    <>
      <TopNavbar />
      <main className={styles.page}>
        <div className={styles.grid}>
          {/* ===== Coluna esquerda — entra no PDF ===== */}
          <section ref={pdfRef} className={styles.exportArea}>
                         <section className={styles.headerSection} aria-label="Cabeçalho da pesquisa">
               <CabecalhoPesquisa autor={autor} data={dataStr} />
               <InformacoesPesquisa
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
            abaAtiva={abaAtiva}
            setAbaAtiva={setAbaAtiva}
            pesquisaId={id}
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
      </main>
    </>
  );
};

export default ResultadosPesquisa;
