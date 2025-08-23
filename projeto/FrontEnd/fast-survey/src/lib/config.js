// Configurações da API
export const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5062';

// Configurações de timeout
export const API_TIMEOUT = 30000; // 30 segundos

// Configurações de retry
export const API_RETRY_ATTEMPTS = 3;
export const API_RETRY_DELAY = 1000; // 1 segundo

// Headers padrão
export const DEFAULT_HEADERS = {
  'Content-Type': 'application/json',
  'Accept': 'application/json'
};

// Configurações de cache
export const CACHE_CONFIG = {
  maxAge: 5 * 60 * 1000, // 5 minutos
  maxItems: 100
};
