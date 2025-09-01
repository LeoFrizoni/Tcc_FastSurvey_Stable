// src/lib/api.js
import axios from 'axios';
import env from '../config/env';

const api = axios.create({
  baseURL: env.REACT_APP_API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
});

// Função para obter token de localStorage ou sessionStorage
const getToken = () => {
  // Primeiro tenta localStorage (persistente)
  let token = localStorage.getItem('token');
  if (token) return token;
  
  // Se não encontrar, tenta sessionStorage (temporário)
  token = sessionStorage.getItem('token');
  return token;
};

// Injeta Bearer se houver
api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
    console.log('Axios Interceptor - Token encontrado, adicionando ao header');
  } else {
    console.log('Axios Interceptor - Nenhum token encontrado');
  }
  console.log('Axios Interceptor - URL da requisição:', config.url);
  return config;
});

// Trata 401: limpa sessão e redireciona ao /login (sem loop)
let isRedirecting = false;
api.interceptors.response.use(
  (res) => res,
  (error) => {
    const status = error?.response?.status;
    if (status === 401 && !isRedirecting) {
      isRedirecting = true;
      // Limpar tanto localStorage quanto sessionStorage
      localStorage.removeItem('token');
      localStorage.removeItem('userId');
      localStorage.removeItem('tipousuarioid');
      sessionStorage.removeItem('token');
      sessionStorage.removeItem('userId');
      sessionStorage.removeItem('tipousuarioid');
      // opcional: guardar rota pretendida
      const here = window.location.pathname + window.location.search;
      if (here && here !== '/login') sessionStorage.setItem('postLoginRedirect', here);
      if (window.location.pathname !== '/login') window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
