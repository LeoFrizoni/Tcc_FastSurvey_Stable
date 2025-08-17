// resultadosPesquisa.jsx (alinhado ao Criar/Preview)
// Respeita estilo: { corFundo, corTexto, fonte } e opcoes { opcaoid, texto }.
// Exibe imagens como miniaturas se houver previewUrl; senão, mostra chips com nome.

import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import ModalQRCode from '../../components/layouts/ModalQrCode';
import styles from './resultadosPesquisa.module.css';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

function normalizeTipo(tipo) {
  return String(tipo || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
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
function buildBlocoInlineStyle(estilo) {
  const { corFundo, corTexto, fonte } = estilo || {};
  const computedText = corTexto || getContrastingTextColor(corFundo);
  return {
    style: {
      backgroundColor: corFundo || 'white',
      color: computedText,
      fontFamily: fonte || 'inherit',
      border: '1px solid var(--border)',
      borderRadius: '12px',
      boxShadow: '0 1px 2px rgba(16,24,40,.04)',
      padding: '14px 16px'
    }
  };
}

function ModalAcoesLateralResultados({ onResponder, onQrCode, onExportarPDF }) {
  return (
    <aside className={styles.actionsPanel} aria-label="Ações">
      <h3 className={styles.actionsTitle}>Ações</h3>
      <button className={styles.primaryBtn} type="button" onClick={onResponder}>Responder pesquisa</button>
      <button className={styles.secondaryBtn} type="button" onClick={onQrCode}>Mostrar QR Code</button>
      <button className={styles.secondaryBtn} type="button" onClick={onExportarPDF}>Exportar PDF</button>
      <p className={styles.smallInfo}>O PDF respeita o layout e quebra em múltiplas páginas.</p>
    </aside>
  );
}

function GaleriaResultados({ imagens }) {
  if (!Array.isArray(imagens) || imagens.length === 0) return null;
  return (
    <div className={styles.previewRow}>
      {imagens.map((img, i) => {
        const src = img.previewUrl; // só existirá se seu backend salvar URL pública (no momento salvamos só nome)
        const label = img.nome || img.file?.name || `imagem-${i}`;
        return src ? (
          <div key={i} className={styles.thumb}>
            <img src={src} alt={label} />
          </div>
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
    if (!pdfRef.current) return;
    try {
      const canvas = await html2canvas(pdfRef.current, { scale: 2, useCORS: true });
      const imgData = canvas.toDataURL('image/png');
      const pdf = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });

      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();
      const imgWidth = pageWidth;
      const imgHeight = (canvas.height * imgWidth) / canvas.width;

      let heightLeft = imgHeight;
      let position = 0;

      pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;

      while (heightLeft > 0) {
        pdf.addPage();
        position = heightLeft - imgHeight;
        pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
        heightLeft -= pageHeight;
      }

      pdf.save('resultado-pesquisa.pdf');
    } catch {
      alert('Erro ao exportar PDF.');
    }
  }

  if (carregando) {
    return (
      <>
        <TopNavbar />
        <div className={styles.page}>
          <div className={styles.grid}>
            <div className={styles.card}>
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
          <div className={styles.errorBox}>
            <h2>Ops!</h2>
            <p>{erro || 'Não foi possível carregar esta pesquisa.'}</p>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <TopNavbar />
      <main className={styles.page}>
        <div className={styles.header}>
          <div className={styles.headerCard}>
            <h1 className={styles.title}>{pesquisa.titulo}</h1>
            {pesquisa.descricao && <p className={styles.subtitle}>{pesquisa.descricao}</p>}
            <span className={styles.badge}>Tipo: {pesquisa.tipoPesquisa?.descricao || 'Indefinido'}</span>
          </div>
        </div>

        <div className={styles.grid}>
          <section ref={pdfRef} className={styles.formArea} aria-label="Estrutura da pesquisa">
            {blocos.map((bloco, index) => {
              const { style: blocoInline } = buildBlocoInlineStyle(bloco?.estilo);
              const tipo = normalizeTipo(bloco.tipo);
              const key = bloco.perguntaid ?? bloco.id ?? index;

              return (
                <div key={key} className={styles.blockWrapper}>
                  <div className={styles.block} style={blocoInline}>
                    <div className={styles.blockHeader}>
                      <span className={styles.qIndex}>{index + 1}.</span>
                      <h4 className={styles.qText}>{bloco.texto}</h4>
                    </div>

                    {/* IMAGENS ENTRE TEXTO E RESPOSTA/OPÇÕES */}
                    <GaleriaResultados imagens={bloco.imagens} />

                    {tipo === 'discursiva' && (
                      <div className={styles.answerBoxMuted}>
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
                  </div>
                </div>
              );
            })}
          </section>

          <ModalAcoesLateralResultados
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
