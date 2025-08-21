// src/pages/perfil/Perfil.jsx
import React, { useEffect, useState } from 'react';
import TopNavbar from '../../components/layouts/TopNavBar';
import { User, Tag, BadgeCheck } from 'lucide-react';
import styles from './perfil.module.css';
import axios from 'axios';

const API_BASE = 'http://localhost:5062';

// Mapeamento de ID para rótulo exibido no front
const mapTipoUsuario = {
  13: 'Usuário Gratuito',
  14: 'Usuário Premium',
  15: 'Administrador'
};

const Perfil = () => {
  const [usuario, setUsuario] = useState({
    nome: '',
    tipoUsuarioId: '',
    status: 'Ativo',
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const buscarUsuario = async () => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          console.warn('Token não encontrado. Redirecione para o login, se necessário.');
          setLoading(false);
          return;
        }

        // Endpoint protegido que lê os claims do JWT
        const resp = await axios.get(`${API_BASE}/api/login/Perfil`, {
          headers: { Authorization: `Bearer ${token}` },
        });

        const data = resp.data || {};
        setUsuario({
          nome: data.usuario || '',
          tipoUsuarioId: data.tipoUsuarioId || '',
          status: 'Ativo',
        });
      } catch (err) {
        console.error('Erro ao buscar dados do usuário:', err);
      } finally {
        setLoading(false);
      }
    };

    buscarUsuario();
  }, []);

  const iniciais = usuario.nome
    ? usuario.nome.split(' ').map((n) => n[0]).join('').toUpperCase()
    : '??';

  // Traduz o tipo de usuário para rótulo amigável
  const tipoUsuarioLabel = mapTipoUsuario[usuario.tipoUsuarioId] || usuario.tipoUsuarioId || '—';

  return (
    <>
      <TopNavbar />
      <div className={styles.page}>
        <header className={styles.header}>
          <h1 className={styles.title}>Meu Perfil</h1>
          <p className={styles.subtitle}>
            Visualize suas informações pessoais e status de conta abaixo.
          </p>
        </header>

        <section className={styles.center}>
          <div className={styles.card}>
            <div className={styles.avatar} aria-label="Avatar do usuário">
              {iniciais}
            </div>

            <ul className={styles.info}>
              <li>
                <User size={18} />
                <strong>Nome:</strong>
                <span>{loading ? '…' : (usuario.nome || '—')}</span>
              </li>
              <li>
                <Tag size={18} />
                <strong>Tipo de Usuário:</strong>
                <span>{loading ? '…' : tipoUsuarioLabel}</span>
              </li>
              <li>
                <BadgeCheck size={18} />
                <strong>Status:</strong>
                <span>{loading ? '…' : (usuario.status || '—')}</span>
              </li>
            </ul>
          </div>
        </section>
      </div>
    </>
  );
};

export default Perfil;
