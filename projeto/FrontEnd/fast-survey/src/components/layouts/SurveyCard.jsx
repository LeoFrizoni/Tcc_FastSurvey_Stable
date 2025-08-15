import React from 'react';
import { Link } from 'react-router-dom';
import { formatDate } from '../../helpers/formatDate';

const SurveyCard = ({ survey }) => {
  return (
    <div style={cardStyle}>
      <h3>{survey.titulo}</h3>
      <p>Criada em: {formatDate(survey.dataCriacao)}</p>
      <div style={{ marginTop: '10px' }}>
        <Link to={`/minhas-pesquisas/${survey.id}`} style={linkStyle}>Ver Resultados</Link> |{' '}
        <Link to={`/minhas-pesquisas/${survey.id}/editar`} style={linkStyle}>Editar</Link>
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
