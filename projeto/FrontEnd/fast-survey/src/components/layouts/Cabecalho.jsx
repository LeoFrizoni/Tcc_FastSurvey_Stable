import React, { useMemo } from "react";
import "./Cabecalho.css";

function formatarDataBR(data) {
  if (!data) return "—";
  try {
    const d = typeof data === "string" || typeof data === "number" ? new Date(data) : data;
    if (Number.isNaN(d?.getTime?.())) return "—";
    return new Intl.DateTimeFormat("pt-BR", { dateStyle: "short" }).format(d);
  } catch {
    return "—";
  }
}

const Cabecalho = ({ autor, data }) => {
  const dataFmt = useMemo(() => formatarDataBR(data), [data]);

  return (
    <div className="bloco-pergunta cabecalho-pesquisa">
      <div className="cabecalho-pergunta">
        <h4 style={{ margin: 0 }}>Cabeçalho da Pesquisa</h4>
      </div>
      <div className="cabecalho-campos">
        <div className="cabecalho-campo">
          <span className="cabecalho-label">Autor:</span>
          <span className="cabecalho-value">{autor || "—"}</span>
        </div>
        <div className="cabecalho-campo">
          <span className="cabecalho-label">Data de criação:</span>
          <span className="cabecalho-value">{dataFmt}</span>
        </div>
      </div>
    </div>
  );
};

export default Cabecalho;
