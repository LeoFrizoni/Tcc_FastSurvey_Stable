
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import styles from './editarPesquisa.module.css';

const EditarPesquisa = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [pesquisa, setPesquisa] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    async function carregarPesquisa() {
      try {
        const { data } = await axios.get(`http://localhost:5062/api/pesquisas/${id}`);
        setPesquisa(data);
      } catch (error) {
        console.error('Erro ao carregar pesquisa:', error);
        setErro('Não foi possível carregar a pesquisa');
      } finally {
        setCarregando(false);
      }
    }

    carregarPesquisa();
  }, [id]);

  if (carregando) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.loading}>Carregando pesquisa...</div>
        </div>
      </>
    );
  }

  if (erro || !pesquisa) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.error}>
            <h2>Erro</h2>
            <p>{erro || 'Pesquisa não encontrada'}</p>
            <button onClick={() => navigate('/minhas-pesquisas')}>
              Voltar para Minhas Pesquisas
            </button>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <TopNavbar />
      <div className={styles.container}>
        <div className={styles.header}>
          <h1>Editar Pesquisa</h1>
          <p>{pesquisa.titulo}</p>
        </div>
        
        <div className={styles.content}>
          <div className={styles.info}>
            <h3>Informações da Pesquisa</h3>
            <p><strong>Título:</strong> {pesquisa.titulo}</p>
            <p><strong>Descrição:</strong> {pesquisa.descricao || 'Sem descrição'}</p>
            <p><strong>Data de Criação:</strong> {new Date(pesquisa.dataCriacao).toLocaleDateString('pt-BR')}</p>
          </div>

          <div className={styles.actions}>
            <button 
              className={styles.primaryButton}
              onClick={() => navigate(`/minhas-pesquisas/resultado/${id}`)}
            >
              Ver Resultados
            </button>
            <button 
              className={styles.secondaryButton}
              onClick={() => navigate(`/responder/${id}`)}
            >
              Testar Pesquisa
            </button>
            <button 
              className={styles.secondaryButton}
              onClick={() => navigate('/minhas-pesquisas')}
            >
              Voltar
            </button>
          </div>

          <div className={styles.note}>
            <h4>Nota sobre Edição</h4>
            <p>
              A edição completa de pesquisas está em desenvolvimento. 
              Por enquanto, você pode visualizar os resultados e testar a pesquisa.
            </p>
          </div>
        </div>
      </div>
    </>
  );
};

export default EditarPesquisa;
