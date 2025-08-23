// src/api.js
import axios from 'axios';
import { API_URL, API_TIMEOUT, DEFAULT_HEADERS } from './config';

const api = axios.create({
  baseURL: API_URL,
  timeout: API_TIMEOUT,
  headers: DEFAULT_HEADERS,
});

// Interceptor para adicionar token de autorização
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para tratar erros de resposta
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      // Token expirado ou inválido
      localStorage.removeItem('token');
      localStorage.removeItem('loginId');
      localStorage.removeItem('userId');
      localStorage.removeItem('usuario');
      localStorage.removeItem('tipousuarioid');
      
      // Redirecionar para login apenas se não estiver já na página de login
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;
