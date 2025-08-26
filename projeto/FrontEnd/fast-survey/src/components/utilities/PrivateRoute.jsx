// components/utilities/PrivateRoute.jsx
import React from 'react';
import { Navigate } from 'react-router-dom';

const PrivateRoute = ({ children }) => {
  const token = localStorage.getItem('token');
  const loginId = localStorage.getItem('userId') || localStorage.getItem('loginId');

  // Verificar se o usuário está autenticado
  if (!token || !loginId) {
    // Redirecionar para login se não estiver autenticado
    return <Navigate to="/login" replace />;
  }

  // Verificar se o token tem o formato básico (3 partes separadas por ponto)
  const tokenParts = token.split('.');
  if (tokenParts.length !== 3) {
    // Token inválido, limpar localStorage e redirecionar
    localStorage.removeItem('token');
    localStorage.removeItem('loginId');
    localStorage.removeItem('userId');
    localStorage.removeItem('usuario');
    localStorage.removeItem('tipousuarioid');
    localStorage.removeItem('TipoUsuarioId');
    return <Navigate to="/login" replace />;
  }

  // Usuário autenticado, renderizar o componente filho
  return children;
};

export default PrivateRoute;
