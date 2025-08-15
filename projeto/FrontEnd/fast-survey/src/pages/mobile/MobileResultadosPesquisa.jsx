import React from 'react';
import { useParams } from 'react-router-dom';
import './mobileMinhasPesquisas.css';

export default function MobileResultadosPesquisa() {
  const { id } = useParams();
  return (
    <div className="mobile-minhas-pesquisas-wrapper">
      <h1>Resultados Pesquisa Mobile #{id}</h1>
      {/* Conteúdo mobile aqui */}
    </div>
  );
}
