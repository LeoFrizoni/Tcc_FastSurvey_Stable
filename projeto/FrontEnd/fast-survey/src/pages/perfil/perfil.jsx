import React, { useEffect, useState } from 'react';
import TopNavbar from '../../components/layouts/TopNavBar';
import { User, Mail, BadgeCheck } from 'lucide-react';
import styles from './perfil.module.css';
import axios from 'axios';

const Perfil = () => {
  const [usuario, setUsuario] = useState({
    nome: '',
    email: '',
    status: 'Ativo'
  });

  useEffect(() => {
    const buscarUsuario = async () => {
      try {
        const userId = localStorage.getItem('userId');
        if (!userId) return;
        const response = await axios.get(`http://localhost:5062/api/login/me/${userId}`);
        setUsuario({
          nome: response.data.nome,
          email: response.data.email,
          status: response.data.status || 'Ativo'
        });
      } catch (error) {
        console.error('Erro ao buscar dados do usuário:', error);
      }
    };
    buscarUsuario();
  }, []);

  const iniciais = usuario.nome
    ? usuario.nome.split(' ').map(n => n[0]).join('').toUpperCase()
    : '??';

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
                <span>{usuario.nome || '—'}</span>
              </li>
              <li>
                <Mail size={18} />
                <strong>Email:</strong>
                <span>{usuario.email || '—'}</span>
              </li>
              <li>
                <BadgeCheck size={18} />
                <strong>Status:</strong>
                <span>{usuario.status || '—'}</span>
              </li>
            </ul>
          </div>
        </section>
      </div>
    </>
  );
};

export default Perfil;
