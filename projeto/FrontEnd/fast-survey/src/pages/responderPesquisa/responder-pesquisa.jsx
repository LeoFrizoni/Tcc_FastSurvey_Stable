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
function ActionsPanel({ onEnviar, onExportar, disableActions, totalRespostas }) {
  return (
    <aside className={`${styles.actionsPanel} ${styles.noPrint}`} aria-label="Ações">
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
         setPesquisa({ ...data, blocos });

         // Buscar nome do autor
         if (data.loginId) {
           console.log('LoginId da pesquisa:', data.loginId);
           const nomeAutor = await getUserName(data.loginId);
           console.log('Nome do autor encontrado:', nomeAutor);
           setAutorNome(nomeAutor);
         }
      } catch (error) {
        console.error('Erro ao carregar pesquisa:', error);
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

  const blocos = useMemo(() => pesquisa?.blocos ?? [], [pesquisa]);

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

  /* =============== Enviar respostas =============== */
  const enviarRespostas = async () => {
    if (!blocos.length) {
      toast.warn('Não há perguntas para enviar.', { position: 'top-center', autoClose: 3000 });
      return;
    }

    const loginId = parseInt(localStorage.getItem('userId'));
    const payload = [];

    blocos.forEach((b) => {
      const perguntaId = b.perguntaid;
      if (!perguntaId) return;
      const r = respostas[perguntaId];
      if (!r) return;

      if (r.tipo === 'discursiva') {
        const texto = String(r.valor || '').trim();
        if (!texto) return;
        payload.push({ tipo: 'discursiva', perguntaId, texto });
      } else if (r.tipo === 'objetiva') {
        const val = r.valor;
        if (val == null) return;
        payload.push({ tipo: 'objetiva', perguntaId, opcoes: [Number(val)] });
      } else if (r.tipo === 'multipla') {
        const arr = Array.isArray(r.valor) ? r.valor.map(Number) : [];
        if (arr.length === 0) return;
        payload.push({ tipo: 'multipla', perguntaId, opcoes: arr });
      }
    });

    if (payload.length === 0) {
      toast.info('Preencha ao menos uma resposta antes de enviar.', { position: 'top-center', autoClose: 3200 });
      return;
    }

    try {
      setEnviando(true);
      for (const item of payload) {
        if (item.tipo === 'discursiva') {
          await api.post(`/api/Respostas/discursiva`, {
            Perguntaid: item.perguntaId, Texto: item.texto, Loginid: loginId, Pesquisaid: Number(id)
          });
        } else {
          await api.post(`/api/Respostas/opcoes`, {
            Perguntaid: item.perguntaId, OpcoesSelecionadas: item.opcoes, Loginid: loginId, Pesquisaid: Number(id)
          });
        }
      }
      toast.success('Respostas enviadas com sucesso!', { position: 'top-center', autoClose: 3000 });
    } catch {
      toast.error('Erro ao enviar respostas. Verifique e tente novamente.', { position: 'top-center', autoClose: 4000 });
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
                  const perguntaId = b.perguntaid ?? b.id ?? index;
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
                              const value = isObj ? Number(op.opcaoid ?? idx) : Number(idx);
                              const label = isObj ? (op.texto ?? String(value)) : String(op);
                              const inputId = `${baseId}-opt-${idx}`;
                              const checked = resp?.valor === value;
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
                              const value = isObj ? Number(op.opcaoid ?? idx) : Number(idx);
                              const label = isObj ? (op.texto ?? String(value)) : String(op);
                              const inputId = `${baseId}-chk-${idx}`;
                              const checked = Array.isArray(resp?.valor) && resp.valor.includes(value);
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
            />
          </div>
        </main>
      </div>
    </>
  );
};

export default ResponderPesquisa;
