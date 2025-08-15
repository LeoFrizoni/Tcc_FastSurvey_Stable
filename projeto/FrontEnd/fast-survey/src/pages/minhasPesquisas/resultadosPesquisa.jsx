

import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import './resultadosPesquisa.css';

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
                  backgroundColor: bloco.tipo === 'discursiva' ? '#7c3aed' : '#fff',
                  color: bloco.tipo === 'discursiva' ? '#fff' : '#000',
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
                      <li key={i} className="opcao-item">{op}</li>
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