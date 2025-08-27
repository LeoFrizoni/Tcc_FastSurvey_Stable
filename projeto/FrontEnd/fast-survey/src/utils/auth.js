// src/utils/auth.js

/**
 * Função centralizada para fazer logout do usuário
 * Remove todas as chaves relacionadas à autenticação do localStorage
 * e redireciona para a página de login
 */
export const logout = () => {
  // Remove todas as chaves relacionadas à autenticação
  localStorage.removeItem('loginId');
  localStorage.removeItem('userId');
  localStorage.removeItem('token');
  localStorage.removeItem('tipousuarioid');
  localStorage.removeItem('tipoUsuarioId');
  
  // Remove chaves relacionadas ao avatar/perfil se existirem
  localStorage.removeItem('avatarUrl');
  localStorage.removeItem('userName');
  
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
  const token = localStorage.getItem('token');
  const loginId = localStorage.getItem('loginId') || localStorage.getItem('userId');
  return !!(token && loginId);
};

/**
 * Função para obter o ID do usuário logado
 * @returns {number|null}
 */
export const getUserId = () => {
  const loginId = localStorage.getItem('loginId') || localStorage.getItem('userId');
  return loginId ? parseInt(loginId, 10) : null;
};

/**
 * Função para obter o token de autenticação
 * @returns {string|null}
 */
export const getToken = () => {
  return localStorage.getItem('token');
};

/**
 * Função para obter o tipo de usuário
 * @returns {number|null}
 */
export const getUserType = () => {
  const tipoUsuarioId = localStorage.getItem('tipousuarioid') || localStorage.getItem('tipoUsuarioId');
  return tipoUsuarioId ? parseInt(tipoUsuarioId, 10) : null;
};
