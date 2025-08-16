import React from "react";
import "./Cabecalho.css";

const Cabecalho = ({ autor, data }) => {
  return (
    <div className="bloco-pergunta cabecalho-pesquisa">
      <div className="cabecalho-pergunta">
        <h4 style={{margin:0}}>Cabeçalho da Pesquisa</h4>
      </div>
      <div className="cabecalho-campos">
        <div className="cabecalho-campo">
          <span className="cabecalho-label">Autor:</span>
          <span className="cabecalho-value">{autor}</span>
        </div>
        <div className="cabecalho-campo">
          <span className="cabecalho-label">Data de criação:</span>
          <span className="cabecalho-value">{data}</span>
        </div>
      </div>
    </div>
  );
};

export default Cabecalho;
