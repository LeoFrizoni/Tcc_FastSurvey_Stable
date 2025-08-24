import React from "react";
import styles from "./informacoes-pesquisa.module.css";

const InformacoesPesquisa = ({ titulo, descricao, tipoPesquisa }) => {
  const tituloSafe = titulo?.trim() || "Sem título";
  const descricaoSafe = descricao?.trim() || "Sem descrição.";
  const tipoSafe = tipoPesquisa?.toString()?.trim() || "—";

  return (
    <div className={styles["info-pesquisa-container"]}>
      <h2 className={styles["info-pesquisa-titulo"]}>{tituloSafe}</h2>
      <p className={styles["info-pesquisa-descricao"]}>{descricaoSafe}</p>
      <span className={styles["info-pesquisa-tipo"]}>
        Tipo: <strong>{tipoSafe}</strong>
      </span>
    </div>
  );
};

export default InformacoesPesquisa;
