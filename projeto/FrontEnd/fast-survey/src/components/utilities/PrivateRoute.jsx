// src/components/utilities/PrivateRoute.jsx
import React from 'react';
import { Navigate } from 'react-router-dom';

const PrivateRoute = ({ children }) => {
  // Verificar primeiro localStorage, depois sessionStorage
  let token = localStorage.getItem('token');
  let loginId = localStorage.getItem('loginId') || localStorage.getItem('userId');
  
  if (!token || !loginId) {
    token = sessionStorage.getItem('token');
    loginId = sessionStorage.getItem('loginId') || sessionStorage.getItem('userId');
  }
  
  console.log('🔍 PrivateRoute - Verificando autenticação');
  console.log('🔍 PrivateRoute - Token:', token ? 'existe' : 'não existe');
  console.log('🔍 PrivateRoute - LoginId:', loginId);
  
  // Verificação mais simples - apenas se existe token e loginId
  if (!token || !loginId) {
    console.log('🔍 PrivateRoute - Sem token ou loginId, redirecionando para /login');
    return <Navigate to="/login" replace />;
  }
  
  console.log('🔍 PrivateRoute - Autenticação OK, renderizando children');
  return children;
};

export default PrivateRoute;
