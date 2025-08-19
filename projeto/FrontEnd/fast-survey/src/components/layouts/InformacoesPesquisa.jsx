import React from "react";
import "./InformacoesPesquisa.css";

const InformacoesPesquisa = ({ titulo, descricao, tipoPesquisa }) => {
  const tituloSafe = titulo?.trim() || "Sem título";
  const descricaoSafe = descricao?.trim() || "Sem descrição.";
  const tipoSafe = tipoPesquisa?.toString()?.trim() || "—";

  return (
    <div className="info-pesquisa-container">
      <h2 className="info-pesquisa-titulo">{tituloSafe}</h2>
      <p className="info-pesquisa-descricao">{descricaoSafe}</p>
      <span className="info-pesquisa-tipo">
        Tipo: <strong>{tipoSafe}</strong>
      </span>
    </div>
  );
};

export default InformacoesPesquisa;
