

import React, { useEffect, useState, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import './responderPesquisa.css';

const ResponderPesquisa = () => {
  const { id } = useParams();
  const [pesquisa, setPesquisa] = useState(null);
  const [respostas, setRespostas] = useState({});
  const [carregando, setCarregando] = useState(true);

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

  // Handlers otimizados
  const handleCheckboxChange = useCallback((perguntaId, opcaoTexto) => {
    setRespostas((prev) => {
      const anteriores = prev[perguntaId] || [];
      const atualizadas = anteriores.includes(opcaoTexto)
        ? anteriores.filter((item) => item !== opcaoTexto)
        : [...anteriores, opcaoTexto];
      return { ...prev, [perguntaId]: atualizadas };
    });
  }, []);

  const handleRadioChange = useCallback((perguntaId, opcaoTexto) => {
    setRespostas((prev) => ({ ...prev, [perguntaId]: opcaoTexto }));
  }, []);

  const handleTextareaChange = useCallback((perguntaId, valor) => {
    setRespostas((prev) => ({ ...prev, [perguntaId]: valor }));
  }, []);

  const enviarRespostas = async () => {
    const corpo = {
      pesquisaId: parseInt(id),
      respostas: Object.entries(respostas).map(([perguntaId, resposta]) => ({
        pergunta: perguntaId,
        resposta: Array.isArray(resposta) ? resposta.join(', ') : resposta
      }))
    };

    try {
      await axios.post('http://localhost:5062/api/respostas', corpo);
      alert('Respostas enviadas com sucesso!');
    } catch (error) {
      console.error('Erro ao enviar respostas:', error);
      alert('Erro ao enviar respostas. Verifique e tente novamente.');
    }
  };

  if (carregando) return <p>Carregando pesquisa...</p>;
  if (!pesquisa) return <p>Pesquisa não encontrada.</p>;

  return (
    <>
      <TopNavbar />
      <div className="responder-bg">
        <div className="responder-pesquisa">
          <h1>{pesquisa.titulo}</h1>
          <p>{pesquisa.descricao}</p>

          {pesquisa.blocos?.map((b) => (
            <div
              key={b.id}
              className={`pergunta${b.tipo === 'discursiva' ? ' pergunta-discursiva' : ''}`}
              style={{
                backgroundColor: b.tipo === 'discursiva' ? '#7c3aed' : '#fff',
                color: b.tipo === 'discursiva' ? '#fff' : '#000',
                fontFamily: b.estilo?.fonte || 'inherit',
                boxShadow: b.tipo === 'discursiva' ? '0 2px 12px rgba(124, 58, 237, 0.15)' : undefined
              }}
            >
              <label><strong>{b.texto}</strong></label>

              {b.tipo === 'multipla' && b.opcoes.map((op, i) => (
                <div key={i}>
                  <input
                    type="checkbox"
                    id={`chk-${b.id}-${i}`}
                    name={`bloco-${b.id}`}
                    value={op}
                    checked={(respostas[b.id] || []).includes(op)}
                    onChange={() => handleCheckboxChange(b.id, op)}
                  />
                  <label htmlFor={`chk-${b.id}-${i}`}>{op}</label>
                </div>
              ))}

              {b.tipo === 'objetiva' && b.opcoes.map((op, i) => (
                <div key={i}>
                  <input
                    type="radio"
                    id={`radio-${b.id}-${i}`}
                    name={`bloco-${b.id}`}
                    value={op}
                    checked={respostas[b.id] === op}
                    onChange={() => handleRadioChange(b.id, op)}
                  />
                  <label htmlFor={`radio-${b.id}-${i}`}>{op}</label>
                </div>
              ))}

              {b.tipo === 'discursiva' && (
                <textarea
                  name={`bloco-${b.id}`}
                  value={respostas[b.id] || ''}
                  onChange={(e) => handleTextareaChange(b.id, e.target.value)}
                  placeholder="Digite sua resposta..."
                  style={{ color: '#222', background: '#ede9fe', border: '1.5px solid #a78bfa' }}
                />
              )}
            </div>
          ))}

          <button type="button" onClick={enviarRespostas} aria-label="Enviar respostas">Enviar</button>
        </div>
      </div>
    </>
  );
};

export default ResponderPesquisa;
