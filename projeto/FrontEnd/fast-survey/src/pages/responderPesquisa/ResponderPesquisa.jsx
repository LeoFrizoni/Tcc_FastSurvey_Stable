import CabecalhoPesquisa from '../../components/layouts/CabecalhoPesquisa';
import InformacoesPesquisa from '../../components/layouts/InformacoesPesquisa';
// src/pages/responder/responderPesquisa.jsx
// Responder (alinhado ao Criar/Preview)
// Respeita estilo salvo: { corFundo, corTexto, fonte } e opcoes { opcaoid, texto }.
// Inclui cabeçalho com Autor e Data.

import React, { useEffect, useState, useCallback, useRef, useMemo } from 'react';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import styles from './responderPesquisa.module.css';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

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

function ActionsPanel({ onEnviar, onExportar, disableActions, totalRespostas }) {
  return (
    <aside className={styles.actionsPanel} aria-label="Ações">
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

function GaleriaPergunta({ imagens }) {
  if (!Array.isArray(imagens) || imagens.length === 0) return null;
  return (
    <div className={styles.previewRow}>
      {imagens.map((img, i) => {
        const src = img.previewUrl;
        const label = img.nome || img.file?.name || `imagem-${i}`;
        return src ? (
          <div key={i} className={styles.thumb}>
            <img src={src} alt={label} loading="lazy" />
          </div>
        ) : (
          <span key={i} className={styles.fileChip}>{label}</span>
        );
      })}
    </div>
  );
}

const ResponderPesquisa = () => {
  const { id } = useParams();
  const [pesquisa, setPesquisa] = useState(null);
  const [respostas, setRespostas] = useState({});
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [anexos, setAnexos] = useState([]);

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
      setCarregando(true);
      setErro('');
      try {
        const res = await axios.get(`http://localhost:5062/api/pesquisas/${id}`);
        const data = res.data;
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
      } catch {
        setErro('Não foi possível carregar a pesquisa. Tente novamente mais tarde.');
      } finally {
        setCarregando(false);
      }
    }
    async function carregarAnexos() {
      try {
        const res = await axios.get(`http://localhost:5062/api/anexos/pesquisa/${id}`);
        setAnexos(res.data || []);
      } catch {
        setAnexos([]);
      }
    }
    carregar();
    carregarAnexos();
  }, [id]);

  const blocos = useMemo(() => pesquisa?.blocos ?? [], [pesquisa]);

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

  const exportarPDF = async () => {
    const container = pdfRef.current;
    if (!container) return;
    const toHide = container.querySelectorAll('.btn-pdf-hide');
    toHide.forEach(el => el && el.classList.add('hide-pdf'));
    try {
      const canvas = await html2canvas(container, {
        scale: 2, useCORS: true, backgroundColor: '#ffffff', windowWidth: document.documentElement.scrollWidth
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

      while (heightLeft > -pageH) {
        pdf.addPage();
        position = heightLeft + margin;
        pdf.addImage(imgData, 'PNG', margin, position, imgW, imgH);
        heightLeft -= pageH;
      }

      const safeTitle = (pesquisa?.titulo || 'resposta-pesquisa').replace(/[\\/:*?"<>|]+/g, '').slice(0, 60);
      pdf.save(`${safeTitle}.pdf`);
      toast.success('PDF exportado com sucesso!', { position: 'top-center', autoClose: 2500 });
    } catch {
      toast.error('Erro ao exportar PDF.', { position: 'top-center', autoClose: 4000 });
    } finally {
      toHide.forEach(el => el && el.classList.remove('hide-pdf'));
    }
  };

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
          await axios.post('http://localhost:5062/api/Respostas/discursiva', {
            PerguntaId: item.perguntaId, Texto: item.texto, LoginId: loginId, PesquisaId: Number(id)
          });
        } else {
          await axios.post('http://localhost:5062/api/Respostas/opcoes', {
            PerguntaId: item.perguntaId, OpcoesSelecionadas: item.opcoes, LoginId: loginId, PesquisaId: Number(id)
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

  // ==== Autor & Data (com fallbacks)
  const autor =
    pesquisa?.login?.usuario ??
    pesquisa?.autor?.nome ??
    pesquisa?.autor ??
    pesquisa?.usuario?.nome ??
    '—';

  const dataRaw =
    pesquisa?.dataCriacao ??
    pesquisa?.dataregistro ??
    pesquisa?.dataRegistro ??
    pesquisa?.createdAt ??
    pesquisa?.criadoEm;

  const dataStr = dataRaw ? new Date(dataRaw).toLocaleDateString('pt-BR') : '—';

  return (
    <>
      <div className={styles.wrapper}>
        <TopNavbar />
        <ToastContainer />
        <main className={styles.page}>
          <div className={styles.header}>
            <section ref={pdfRef} className={styles.headerSection} aria-label="Cabeçalho da pesquisa">
              <CabecalhoPesquisa autor={autor} data={dataStr} />
              <InformacoesPesquisa
                titulo={pesquisa.titulo}
                descricao={pesquisa.descricao}
                tipoPesquisa={pesquisa?.tipoPesquisa?.descricao ?? '—'}
              />
              {anexos.length > 0 && (
                <div className={styles.attachments}>
                  <h3>Anexos da Pesquisa</h3>
                  <ul>
                    {anexos.map(anexo => (
                      <li key={anexo.anexoid}>
                        <a
                          className={styles.attachmentLink}
                          href={`http://localhost:5062/Uploads/${anexo.nome}${anexo.extensao}`}
                          download
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          📎 {anexo.nome}{anexo.extensao}
                        </a>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </section>
          </div>
          <div className={styles.grid}>
            <section className={styles.formArea} aria-label="Formulário da pesquisa">
              {blocos.map((b, index) => {
                const tipo = normalizeTipo(b.tipo);
                const perguntaId = b.perguntaid ?? b.id ?? index;
                const { style: blocoInline } = buildBlocoInlineStyle(b?.estilo);
                const resp = respostas[perguntaId];
                const baseId = `pergunta-${perguntaId}`;
                return (
                  <div key={perguntaId} className={styles.blockWrapper}>
                    <div className={styles.block} style={blocoInline}>
                      <div className={styles.blockHeader}>
                        <span className={styles.qIndex} aria-hidden>{index + 1}.</span>
                        <h4 className={styles.qText}>{b.texto}</h4>
                      </div>
                      <GaleriaPergunta imagens={b.imagens} />
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
