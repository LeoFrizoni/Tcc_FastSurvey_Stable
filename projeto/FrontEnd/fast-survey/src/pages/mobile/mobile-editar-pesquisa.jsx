import React from 'react';
import { useParams } from 'react-router-dom';
import styles from './mobile-editar-pesquisa.module.css';

export default function MobileEditarPesquisa() {
  const { id } = useParams();
  return (
    <div className={styles["mobile-editar-wrapper"]}>
      <h1>Editar Pesquisa Mobile #{id}</h1>
      {/* Conteúdo mobile aqui */}
    </div>
  );
}
