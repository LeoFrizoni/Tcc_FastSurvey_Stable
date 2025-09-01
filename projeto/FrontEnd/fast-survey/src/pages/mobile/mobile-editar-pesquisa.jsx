import React from 'react';
import { useParams } from 'react-router-dom';
import styles from './mobile-editar-pesquisa.module.css';

export default function MobileEditarPesquisa() {
  const { id: rawId } = useParams();
  const id = rawId?.replace(/[^0-9]/g, ''); // Remove caracteres não numéricos
  return (
    <div className={styles["mobile-editar-wrapper"]}>
      <h1>Editar Pesquisa Mobile #{id}</h1>
      {/* Conteúdo mobile aqui */}
    </div>
  );
}
