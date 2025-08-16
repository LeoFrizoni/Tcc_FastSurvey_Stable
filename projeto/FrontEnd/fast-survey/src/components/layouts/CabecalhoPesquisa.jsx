import React from "react";
import "../perguntas/pergunta.css";

const CabecalhoPesquisa = ({ autor, data, selecionado, style, onClick }) => {
  return (
    <div
      className={`bloco-pergunta${selecionado ? " selecionado" : ""}`}
      style={{
        background: "linear-gradient(90deg, #f8f9fd 0%, #eef2fa 100%)",
        border: "1.5px solid #e0e7ef",
        borderRadius: "14px",
        boxShadow: "0 2px 12px rgba(44, 62, 80, 0.06)",
        marginBottom: "2rem",
        padding: "1.8rem 2.2rem",
        fontFamily: "'Segoe UI', 'Inter', Arial, sans-serif",
        ...style
      }}
      onClick={e => {
        e.stopPropagation();
        if (onClick) onClick();
      }}
    >
      <div className="cabecalho-pergunta" style={{display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "1.1rem"}}>
        <h4 style={{ margin: 0, fontWeight: 700, color: "#2d3875", fontSize: "1.25rem", letterSpacing: "0.5px" }}>Cabeçalho da Pesquisa</h4>
      </div>
      <div style={{ display: "flex", gap: "3rem", flexWrap: "wrap" }}>
        <div style={{ minWidth: 180 }}>
          <span style={{ fontWeight: 600, color: "#3a3a3a", marginRight: 8, fontSize: "1.05rem" }}>Autor:</span>
          <span style={{ color: "#6c63ff", fontWeight: 500, fontSize: "1.05rem" }}>{autor}</span>
        </div>
        <div style={{ minWidth: 180 }}>
          <span style={{ fontWeight: 600, color: "#3a3a3a", marginRight: 8, fontSize: "1.05rem" }}>Data de criação:</span>
          <span style={{ color: "#6c63ff", fontWeight: 500, fontSize: "1.05rem" }}>{data}</span>
        </div>
      </div>
    </div>
  );
};

export default CabecalhoPesquisa;
