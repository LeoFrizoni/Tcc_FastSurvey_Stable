import React from "react";
import "./InformacoesPesquisa.css";

const InformacoesPesquisa = ({ titulo, descricao, tipoPesquisa }) => {
  return (
    <div className="info-pesquisa-container">
      <h2 className="info-pesquisa-titulo">{titulo}</h2>
      <p className="info-pesquisa-descricao">{descricao}</p>
      <span className="info-pesquisa-tipo">Tipo: <strong>{tipoPesquisa}</strong></span>
    </div>
  );
};

export default InformacoesPesquisa;
