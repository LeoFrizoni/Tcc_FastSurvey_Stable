import axios from 'axios';

// Configurar interceptor do axios para adicionar token automaticamente
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
      console.log('🔧 Axios Interceptor - Token adicionado automaticamente');
    } else {
      console.log('⚠️ Axios Interceptor - Nenhum token encontrado');
    }
    return config;
  },
  (error) => {
    console.error('❌ Axios Interceptor - Erro na requisição:', error);
    return Promise.reject(error);
  }
);

// Interceptor para tratar erros de autenticação
axios.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      console.log('❌ Axios Interceptor - Erro 401 - Token inválido ou expirado');
      console.log('❌ Axios Interceptor - URL:', error.config?.url);
      console.log('❌ Axios Interceptor - Headers:', error.config?.headers);
    }
    return Promise.reject(error);
  }
);

export default axios;

