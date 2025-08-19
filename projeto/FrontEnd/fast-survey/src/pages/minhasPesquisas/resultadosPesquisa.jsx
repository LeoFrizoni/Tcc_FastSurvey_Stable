import CabecalhoPesquisa from '../../components/layouts/CabecalhoPesquisa';
import InformacoesPesquisa from '../../components/layouts/InformacoesPesquisa';

import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import ModalQRCode from '../../components/layouts/ModalQrCode';
import styles from './resultadosPesquisa.module.css';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

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

function ActionsAside({ onResponder, onQrCode, onExportarPDF }) {
  return (
    <aside className={`${styles.actionsPanel} ${styles.noPrint}`} aria-label="Ações">
      <h3 className={styles.actionsTitle}>Ações</h3>
      <button className={styles.primaryBtn} type="button" onClick={onResponder}>
        Responder pesquisa
      </button>
      <button className={styles.secondaryBtn} type="button" onClick={onQrCode}>
        Mostrar QR Code
      </button>
      <button className={styles.secondaryBtn} type="button" onClick={onExportarPDF}>
        Exportar PDF
      </button>
      <p className={styles.smallInfo}>O PDF respeita o layout e quebra em múltiplas páginas.</p>
    </aside>
  );
}

function GaleriaResultados({ imagens }) {
  if (!Array.isArray(imagens) || imagens.length === 0) return null;
  return (
    <div className={styles.previewRow}>
      {imagens.map((img, i) => {
        const src = img.previewUrl;
        const label = img.nome || img.file?.name || `imagem-${i}`;
        return src ? (
          <figure key={i} className={styles.thumb}>
            <img src={src} alt={label} loading="lazy" />
          </figure>
        ) : (
          <span key={i} className={styles.fileChip}>{label}</span>
        );
      })}
    </div>
  );
}

const ResultadosPesquisa = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [pesquisa, setPesquisa] = useState(null);
  const [mostrarModalQr, setMostrarModalQr] = useState(false);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const pdfRef = useRef(null);

  useEffect(() => {
    async function buscarPesquisa() {
      setCarregando(true);
      setErro('');
      try {
        const response = await axios.get(`http://localhost:5062/api/pesquisas/${id}`);
        const data = response.data;
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
    buscarPesquisa();
  }, [id]);

  const blocos = useMemo(() => pesquisa?.blocos ?? [], [pesquisa]);

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
    pesquisa?.dataCriacao ??
    pesquisa?.dataregistro ??
    pesquisa?.dataRegistro ??
    pesquisa?.createdAt ??
    pesquisa?.criadoEm;

  const dataStr = dataRaw ? new Date(dataRaw).toLocaleDateString('pt-BR') : '—';

  return (
    <>
      <TopNavbar />
      <main className={styles.page}>
        {/* GRID: coluna esquerda (conteúdo/PDF) + coluna direita (painel) */}
        <div className={styles.grid}>
          {/* ===== Coluna esquerda — entra no PDF ===== */}
          <section ref={pdfRef} className={styles.exportArea}>
            <section className={styles.headerSection} aria-label="Cabeçalho da pesquisa">
              <CabecalhoPesquisa autor={autor} data={dataStr} />
              <InformacoesPesquisa
                titulo={pesquisa.titulo}
                descricao={pesquisa.descricao}
                tipoPesquisa={pesquisa.tipoPesquisa?.descricao || 'Indefinido'}
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
                      <GaleriaResultados imagens={bloco.imagens} />
                      {tipo === 'discursiva' && (
                        <div className={styles.answerBoxMuted} aria-label="Resposta do usuário (discursiva)">
                          Resposta do usuário...
                        </div>
                      )}
                      {(tipo === 'objetiva' || tipo === 'multipla') && Array.isArray(bloco.opcoes) && (
                        <ul className={styles.optionsList}>
                          {bloco.opcoes.map((op, i) => {
                            const label = (op && typeof op === 'object') ? (op.texto ?? String(i + 1)) : String(op);
                            return (
                              <li key={op?.opcaoid ?? i} className={styles.optionChip}>
                                {label}
                              </li>
                            );
                          })}
                        </ul>
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
          />
        </div>

        <ModalQRCode
          isOpen={mostrarModalQr}
          onClose={() => setMostrarModalQr(false)}
          qrUrl={`${window.location.origin}/responder/${id}`}
        />
      </main>
    </>
  );
};

export default ResultadosPesquisa;
