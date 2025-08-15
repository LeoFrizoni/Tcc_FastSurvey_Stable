import React, { useEffect, useState, useCallback, useRef } from 'react';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import '../perfil/perfil.css';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

// cor de texto automática baseada no fundo
function getContrastingTextColor(bgColor) {
  if (!bgColor) return '#222';
  let color = bgColor.replace('#', '');
  if (color.length === 3) color = color.split('').map(c => c + c).join('');
  if (color.length !== 6) return '#222';
  const r = parseInt(color.substr(0, 2), 16);
  const g = parseInt(color.substr(2, 2), 16);
  const b = parseInt(color.substr(4, 2), 16);
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return luminance > 0.5 ? '#222' : '#fff';
}

// utilitário para esconder elementos na exportação do PDF
function hideForPDF(elements) {
  elements.forEach(el => el && el.classList.add('hide-pdf'));
}
function showForPDF(elements) {
  elements.forEach(el => el && el.classList.remove('hide-pdf'));
}

const ResponderPesquisa = () => {
  const { id } = useParams();
  const [pesquisa, setPesquisa] = useState(null);
  const [respostas, setRespostas] = useState({});
  const [carregando, setCarregando] = useState(true);
  const pdfRef = useRef();

  // Exportar PDF
  const exportarPDF = async () => {
    if (!pdfRef.current) return;
    const botoes = pdfRef.current.querySelectorAll('.btn-pdf-hide');
    hideForPDF(botoes);
    try {
      const canvas = await html2canvas(pdfRef.current);
      const imgData = canvas.toDataURL('image/png');
      const pdf = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();
      const ratio = Math.min(pageWidth / canvas.width, pageHeight / canvas.height);
      const imgWidth = canvas.width * ratio;
      const imgHeight = canvas.height * ratio;
      pdf.addImage(imgData, 'PNG', 0, 0, imgWidth, imgHeight);
      pdf.save('resposta-pesquisa.pdf');
    } catch (err) {
      toast.error('Erro ao exportar PDF.', { position: 'top-center', autoClose: 4000 });
    } finally {
      showForPDF(botoes);
    }
  };

  // CSS p/ hide-pdf
  useEffect(() => {
    if (!document.getElementById('hide-pdf-style')) {
      const style = document.createElement('style');
      style.id = 'hide-pdf-style';
      style.innerHTML = `.hide-pdf { display: none !important; }`;
      document.head.appendChild(style);
    }
  }, []);

  // Carregar pesquisa
  useEffect(() => {
    const carregarPesquisa = async () => {
      try {
        const res = await axios.get(`http://localhost:5062/api/pesquisas/${id}`);
        const data = res.data;
        if (data.templateJson) {
          try {
            const blocos = JSON.parse(data.templateJson);
            setPesquisa({ ...data, blocos });
          } catch (e) {
            console.warn('Erro ao interpretar TemplateJson:', e);
            setPesquisa(data);
          }
        } else {
          setPesquisa(data);
        }
        setCarregando(false);
      } catch (error) {
        console.error('Erro ao carregar pesquisa:', error);
        setCarregando(false);
      }
    };
    carregarPesquisa();
  }, [id]);

  // Handlers de resposta
  const handleCheckboxChange = useCallback((perguntaId, opcaoValor) => {
    setRespostas(prev => {
      const anterior = prev[perguntaId];
      const anteriores = Array.isArray(anterior?.valor) ? anterior.valor : [];
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

  // Enviar respostas
  const enviarRespostas = async () => {
    const loginId = parseInt(localStorage.getItem('userId'));
    const corpo = Object.entries(respostas).map(([perguntaId, respostaObj]) => {
      const textoResposta = Array.isArray(respostaObj.valor) ? respostaObj.valor.join(',') : respostaObj.valor;
      return {
        perguntaid: parseInt(perguntaId),
        texto: textoResposta,
        dataresposta: new Date().toISOString(),
        loginid: loginId,
        pesquisaid: parseInt(id)
      };
    });

    try {
      await axios.post('http://localhost:5062/api/respostas', corpo);
      toast.success('Respostas enviadas com sucesso!', { position: 'top-center', autoClose: 3000 });
    } catch (error) {
      console.error('Erro ao enviar respostas:', error);
      toast.error('Erro ao enviar respostas. Verifique e tente novamente.', { position: 'top-center', autoClose: 4000 });
    }
  };

  if (carregando) return <p>Carregando pesquisa...</p>;
  if (!pesquisa) return <p>Pesquisa não encontrada.</p>;

  return (
    <>
      <TopNavbar />
      <ToastContainer />
      <div className="responder-bg">
        <div className="resultados-container responder-pesquisa" ref={pdfRef}>
          <div className="header-pesquisa">
            <h1>{pesquisa.titulo}</h1>
            <p className="descricao">{pesquisa.descricao}</p>
            <span className="tag-tipo">Tipo: {pesquisa?.tipoPesquisa?.descricao ?? '—'}</span>
          </div>

          <div className="perguntas-lista">
            {pesquisa.blocos?.map((b, index) => {
              const tipo = String(b.tipo || '')
                .normalize('NFD')
                .replace(/[\u0300-\u036f]/g, '')
                .toLowerCase();

              const key = b.perguntaid ?? b.id ?? index;

              const corFundo = b?.estilo?.corFundo || '';
              const corTexto = b?.estilo?.corTexto || getContrastingTextColor(corFundo);
              const fonte = b?.estilo?.fonte || '';

              const blocoStyle = {
                backgroundColor: corFundo || undefined,
                color: corTexto || undefined,
                fontFamily: fonte || undefined
              };

              // normaliza opção
              const opt = (op, idx) => {
                const isObj = op && typeof op === 'object';
                const value = isObj ? (op.opcaoid ?? op.id ?? idx) : idx;
                const label = isObj ? (op.texto ?? String(value)) : String(op);
                const optKey = isObj ? (op.opcaoid ?? op.id ?? idx) : idx;
                return { value, label, optKey };
              };

              if (tipo === 'discursiva') {
                return (
                  <div key={key} className="pergunta-card pergunta-card-discursiva" style={blocoStyle}>
                    <p className="titulo-pergunta">{b.texto}</p>
                    <textarea
                      style={{ width: '100%', padding: 12, borderRadius: 8, border: '1px solid #ddd' }}
                      onChange={(e) => handleTextareaChange(key, e.target.value)}
                    />
                  </div>
                );
              }

              if (tipo === 'objetiva') {
                const resp = respostas[key];
                return (
                  <div key={key} className="pergunta-card" style={blocoStyle}>
                    <p className="titulo-pergunta">{b.texto}</p>
                    {Array.isArray(b.opcoes) && b.opcoes.map((op, idx) => {
                      const { value, label, optKey } = opt(op, idx);
                      const checked = resp?.valor === value;
                      return (
                        <label key={optKey} style={{ display: 'block', marginBottom: 6, cursor: 'pointer' }}>
                          <input
                            type="radio"
                            name={`pergunta-${key}`}
                            checked={!!checked}
                            onChange={() => handleRadioChange(key, value)}
                            style={{ marginRight: 8 }}
                          />
                          <span>{label}</span>
                        </label>
                      );
                    })}
                  </div>
                );
              }

              if (tipo === 'multipla') {
                const resp = respostas[key];
                return (
                  <div key={key} className="pergunta-card" style={blocoStyle}>
                    <p className="titulo-pergunta">{b.texto}</p>
                    {Array.isArray(b.opcoes) && b.opcoes.map((op, idx) => {
                      const { value, label, optKey } = opt(op, idx);
                      const checked = Array.isArray(resp?.valor) && resp.valor.includes(value);
                      return (
                        <label key={optKey} style={{ display: 'block', marginBottom: 6, cursor: 'pointer' }}>
                          <input
                            type="checkbox"
                            checked={!!checked}
                            onChange={() => handleCheckboxChange(key, value)}
                            style={{ marginRight: 8 }}
                          />
                          <span>{label}</span>
                        </label>
                      );
                    })}
                  </div>
                );
              }

              // fallback
              return (
                <div key={key} className="pergunta-card" style={blocoStyle}>
                  <p className="titulo-pergunta">{b.texto}</p>
                </div>
              );
            })}
          </div>
        </div>

        <div style={{ marginTop: 16 }}>
          <button className="btn-responder btn-pdf-hide" onClick={enviarRespostas}>Enviar Respostas</button>
          <button className="btn-responder btn-pdf-hide" style={{ marginTop: 8 }} onClick={exportarPDF}>Exportar PDF</button>
        </div>
      </div>
    </>
  );
};

export default ResponderPesquisa;