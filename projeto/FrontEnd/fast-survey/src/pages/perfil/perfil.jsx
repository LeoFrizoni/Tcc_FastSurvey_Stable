
import React, { useEffect, useState } from 'react';
import TopNavbar from '../../components/layouts/TopNavBar';
import { User, Mail, BadgeCheck } from 'lucide-react';
import './perfil.css';
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
        if (!userId) {
          console.warn('ID do usuário não encontrado no localStorage.');
          return;
        }

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

  return (
    <>
      <TopNavbar />
      <div className="perfil-container">
        <h1>Meu Perfil</h1>
        <p>Visualize suas informações pessoais e status de conta abaixo.</p>

        <div className="perfil-card">
          <div className="perfil-avatar" aria-label="Avatar do usuário">
            {usuario.nome
              ? usuario.nome
                  .split(' ')
                  .map((n) => n[0])
                  .join('')
                  .toUpperCase()
              : '??'}
          </div>
          <ul className="perfil-dados">
            <li>
              <User size={18} /> <strong>Nome:</strong> {usuario.nome}
            </li>
            <li>
              <Mail size={18} /> <strong>Email:</strong> {usuario.email}
            </li>
            <li>
              <BadgeCheck size={18} /> <strong>Status:</strong> {usuario.status}
            </li>
          </ul>
        </div>
      </div>
    </>
  );
};

export default Perfil;
