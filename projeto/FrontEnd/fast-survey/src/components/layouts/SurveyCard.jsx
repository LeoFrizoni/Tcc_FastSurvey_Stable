import React from 'react';
import { Link } from 'react-router-dom';
import { formatDate } from '../../helpers/formatDate';

const SurveyCard = ({ survey = {} }) => {
  const id = survey.id ?? survey.pesquisaId ?? survey.PesquisaId;
  const titulo = survey.titulo || "Sem título";
  const data = survey.dataCriacao || survey.criadoEm || survey.createdAt || null;

  return (
    <div style={cardStyle} aria-label={`Card da pesquisa ${titulo}`}>
      <h3 style={{ marginTop: 0 }}>{titulo}</h3>
      <p>Criada em: {data ? formatDate(data) : "—"}</p>

      <div style={{ marginTop: '10px' }}>
        {id != null ? (
          <>
            <Link to={`/minhas-pesquisas/${id}`} style={linkStyle}>Ver Resultados</Link>
            {' '}|{' '}
            <Link to={`/minhas-pesquisas/${id}/editar`} style={linkStyle}>Editar</Link>
          </>
        ) : (
          <span style={{ color: '#999' }}>ID não disponível</span>
        )}
      </div>
    </div>
  );
};

const cardStyle = {
  border: '1px solid #ccc',
  borderRadius: '10px',
  padding: '1rem',
  marginBottom: '1rem',
  backgroundColor: '#f9f9f9',
};

const linkStyle = {
  textDecoration: 'none',
  color: '#007bff',
  fontWeight: 'bold',
};

export default SurveyCard;
