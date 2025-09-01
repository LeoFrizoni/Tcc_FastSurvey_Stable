// src/utils/auth.js

/**
 * Função centralizada para fazer logout do usuário
 * Remove todas as chaves relacionadas à autenticação do localStorage
 * e redireciona para a página de login
 */
export const logout = () => {
  // Remove todas as chaves relacionadas à autenticação do localStorage
  localStorage.removeItem('loginId');
  localStorage.removeItem('userId');
  localStorage.removeItem('token');
  localStorage.removeItem('tipousuarioid');
  localStorage.removeItem('tipoUsuarioId');
  
  // Remove chaves relacionadas ao avatar/perfil se existirem
  localStorage.removeItem('avatarUrl');
  localStorage.removeItem('userName');
  
  // Remove todas as chaves relacionadas à autenticação do sessionStorage
  sessionStorage.removeItem('loginId');
  sessionStorage.removeItem('userId');
  sessionStorage.removeItem('token');
  sessionStorage.removeItem('tipousuarioid');
  sessionStorage.removeItem('tipoUsuarioId');
  
  // Remove chaves de cache específicas do FastSurvey
  const keysToRemove = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (key && (key.startsWith('fs_') || key.includes('fastsurvey') || key.includes('FastSurvey'))) {
      keysToRemove.push(key);
    }
  }
  
  // Remove as chaves de cache encontradas
  keysToRemove.forEach(key => localStorage.removeItem(key));
  
  // Redireciona para login
  window.location.href = '/login';
};

/**
 * Função para verificar se o usuário está logado
 * @returns {boolean}
 */
export const isLoggedIn = () => {
  // Verifica primeiro localStorage, depois sessionStorage
  let token = localStorage.getItem('token');
  let loginId = localStorage.getItem('loginId') || localStorage.getItem('userId');
  
  if (!token || !loginId) {
    token = sessionStorage.getItem('token');
    loginId = sessionStorage.getItem('loginId') || sessionStorage.getItem('userId');
  }
  
  return !!(token && loginId);
};

/**
 * Função para obter o ID do usuário logado
 * @returns {number|null}
 */
export const getUserId = () => {
  // Verifica primeiro localStorage, depois sessionStorage
  let loginId = localStorage.getItem('loginId') || localStorage.getItem('userId');
  
  if (!loginId) {
    loginId = sessionStorage.getItem('loginId') || sessionStorage.getItem('userId');
  }
  
  return loginId ? parseInt(loginId, 10) : null;
};

/**
 * Função para obter o token de autenticação
 * @returns {string|null}
 */
export const getToken = () => {
  // Verifica primeiro localStorage, depois sessionStorage
  let token = localStorage.getItem('token');
  
  if (!token) {
    token = sessionStorage.getItem('token');
  }
  
  return token;
};

/**
 * Função para obter o tipo de usuário
 * @returns {number|null}
 */
export const getUserType = () => {
  // Verifica primeiro localStorage, depois sessionStorage
  let tipoUsuarioId = localStorage.getItem('tipousuarioid') || localStorage.getItem('tipoUsuarioId');
  
  if (!tipoUsuarioId) {
    tipoUsuarioId = sessionStorage.getItem('tipousuarioid') || sessionStorage.getItem('tipoUsuarioId');
  }
  
  return tipoUsuarioId ? parseInt(tipoUsuarioId, 10) : null;
};
