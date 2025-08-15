import React from 'react';
import { useParams } from 'react-router-dom';
import './mobileEditarPesquisa.css';

export default function MobileEditarPesquisa() {
  const { id } = useParams();
  return (
    <div className="mobile-editar-wrapper">
      <h1>Editar Pesquisa Mobile #{id}</h1>
      {/* Conteúdo mobile aqui */}
    </div>
  );
}
