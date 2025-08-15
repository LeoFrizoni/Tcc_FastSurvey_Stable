
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import './resultadosPesquisa.css';

import React, { useEffect, useState } from 'react';
// Função para escolher cor de texto automática baseada na cor de fundo
function getContrastingTextColor(bgColor) {
  if (!bgColor) return '#222';
  let color = bgColor.replace('#', '');
  if (color.length === 3) {
    color = color.split('').map(c => c + c).join('');
  }
  if (color.length !== 6) return '#222';
  const r = parseInt(color.substr(0,2),16);
  const g = parseInt(color.substr(2,2),16);
  const b = parseInt(color.substr(4,2),16);
  const luminance = (0.299*r + 0.587*g + 0.114*b)/255;
  return luminance > 0.5 ? '#222' : '#fff';
}
const ResultadosPesquisa = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [pesquisa, setPesquisa] = useState(null);

  useEffect(() => {
    const buscarPesquisa = async () => {
      try {
        const response = await axios.get(`http://localhost:5062/api/pesquisas/${id}`);
        const data = response.data;
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
      } catch (error) {
        console.error('Erro ao buscar a pesquisa:', error);
      }
    };

    buscarPesquisa();
  }, [id]);

  if (!pesquisa) {
    return <div className="carregando">Carregando pesquisa...</div>;
  }

  return (
    <>
      <TopNavbar />
      <div className="resultados-bg">
        <div className="resultados-container">
          <div className="header-pesquisa">
            <h1>{pesquisa.titulo}</h1>
            <p className="descricao">{pesquisa.descricao}</p>
            <span className="tag-tipo">Tipo: {pesquisa.tipoPesquisa?.descricao || 'Indefinido'}</span>
          </div>

          <div className="perguntas-lista">
            {pesquisa.blocos?.map((bloco, index) => (
              <div
                key={bloco.id}
                className={`pergunta-card${bloco.tipo === 'discursiva' ? ' pergunta-card-discursiva' : ''}`}
                style={{
                  backgroundColor: bloco.estilo?.corFundo || 'transparent',
                  color: bloco.estilo?.corTexto || getContrastingTextColor(bloco.estilo?.corFundo),
                  fontFamily: bloco.estilo?.fonte || 'inherit',
                  boxShadow: bloco.tipo === 'discursiva' ? '0 2px 12px rgba(124, 58, 237, 0.15)' : undefined
                }}
              >
                <h3 className="titulo-pergunta">
                  {index + 1}. {bloco.texto}
                </h3>

                {bloco.tipo === 'discursiva' ? (
                  <p className="resposta-discursiva" style={{ color: '#e0e0e0', fontStyle: 'italic', marginTop: 8 }}>
                    Resposta: (campo discursivo)
                  </p>
                ) : (
                  <ul className="lista-opcoes">
                    {bloco.opcoes?.map((op, i) => (
                      <li key={op.opcaoid ?? i} className="opcao-item">{typeof op === 'object' && op !== null ? op.texto : String(op)}</li>
                    ))}
                  </ul>
                )}
              </div>
            ))}
          </div>

          <button className="btn-responder" type="button" aria-label="Responder Pesquisa" onClick={() => navigate(`/responder/${id}`)}>
            Responder Pesquisa
          </button>
        </div>
      </div>
    </>
  );
};

export default ResultadosPesquisa;