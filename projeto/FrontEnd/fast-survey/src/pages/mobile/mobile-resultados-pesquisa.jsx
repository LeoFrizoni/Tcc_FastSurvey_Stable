import React from 'react';
import { useParams } from 'react-router-dom';
import styles from './mobile-resultados-pesquisa.module.css';

export default function MobileResultadosPesquisa() {
  const { id } = useParams();
  return (
    <div className={styles["mobile-resultados-pesquisa-wrapper"]}>
      <h1>Resultados Pesquisa Mobile #{id}</h1>
      {/* Conteúdo mobile aqui */}
    </div>
  );
}
