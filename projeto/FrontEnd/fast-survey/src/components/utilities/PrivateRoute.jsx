// src/components/utilities/PrivateRoute.jsx
import React from 'react';
import { Navigate } from 'react-router-dom';
import { logout } from '../../utils/auth';

function isValidJwt(tk) {
  // Verifica se o token existe
  if (!tk || typeof tk !== 'string') {
    console.log('❌ Token não existe ou não é string');
    return false;
  }

  // Verifica se tem 3 partes
  const parts = tk.split('.');
  if (parts.length !== 3) {
    console.log('❌ Token não tem 3 partes:', parts.length);
    return false;
  }

  try {
    // Decodifica o payload de forma mais robusta
    const payload = parts[1];
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    const parsed = JSON.parse(decoded);
    
    console.log('✅ Token decodificado:', parsed);
    
    // Verifica expiração se existir
    if (parsed?.exp) {
      const now = Date.now() / 1000;
      const exp = parsed.exp;
      const isExpired = now > exp;
      
      console.log('⏰ Verificação de expiração:');
      console.log('  - Agora:', new Date(now * 1000));
      console.log('  - Expira:', new Date(exp * 1000));
      console.log('  - Expirado:', isExpired);
      
      if (isExpired) {
        console.log('❌ Token expirado');
        return false;
      }
    } else {
      console.log('⚠️ Token sem expiração definida - aceitando');
    }
    
    // Verifica se tem claims essenciais
    const hasLoginId = parsed.LoginId || parsed.loginId;
    const hasTipoUsuarioId = parsed.TipoUsuarioId || parsed.tipoUsuarioId;
    
    console.log('🔍 Claims essenciais:');
    console.log('  - LoginId:', hasLoginId);
    console.log('  - TipoUsuarioId:', hasTipoUsuarioId);
    
    if (!hasLoginId || !hasTipoUsuarioId) {
      console.log('❌ Token sem claims essenciais');
      return false;
    }
    
    return true;
  } catch (error) {
    console.log('❌ Erro ao decodificar token:', error);
    return false;
  }
}

const PrivateRoute = ({ children }) => {
  const token = localStorage.getItem('token');
  console.log('🔍 PrivateRoute - Verificando token:', token ? 'existe' : 'não existe');
  
  if (!isValidJwt(token)) {
    console.log('🔍 PrivateRoute - Token inválido, limpando localStorage e redirecionando para /login');
    logout();
    return <Navigate to="/login" replace />;
  }
  
  console.log('🔍 PrivateRoute - Token válido, renderizando children');
  return children;
};

export default PrivateRoute;
