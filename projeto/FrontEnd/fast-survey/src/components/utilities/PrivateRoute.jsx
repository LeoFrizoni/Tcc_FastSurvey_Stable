// components/utilities/PrivateRoute.jsx
import React from 'react';
import { Navigate } from 'react-router-dom';

const PrivateRoute = ({ children }) => {
  const token = localStorage.getItem('token');
  const loginId = localStorage.getItem('loginId') || localStorage.getItem('userId');

  // Verificar se o usuário está autenticado
  if (!token || !loginId) {
    // Redirecionar para login se não estiver autenticado
    return <Navigate to="/login" replace />;
  }

  // Verificar se o token não está expirado (opcional)
  try {
    const tokenData = JSON.parse(atob(token.split('.')[1]));
    const currentTime = Date.now() / 1000;
    
    if (tokenData.exp && tokenData.exp < currentTime) {
      // Token expirado, limpar localStorage e redirecionar
      localStorage.removeItem('token');
      localStorage.removeItem('loginId');
      localStorage.removeItem('userId');
      localStorage.removeItem('usuario');
      localStorage.removeItem('tipousuarioid');
      return <Navigate to="/login" replace />;
    }
  } catch (error) {
    // Se não conseguir decodificar o token, redirecionar para login
    console.error('Erro ao verificar token:', error);
    localStorage.removeItem('token');
    localStorage.removeItem('loginId');
    localStorage.removeItem('userId');
    localStorage.removeItem('usuario');
    localStorage.removeItem('tipousuarioid');
    return <Navigate to="/login" replace />;
  }

  // Usuário autenticado, renderizar o componente filho
  return children;
};

export default PrivateRoute;
