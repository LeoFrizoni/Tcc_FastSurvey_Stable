// Importar axios configurado com interceptors
import './axios';

// Configurações principais da aplicação FastSurvey
export const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5062';

// Google OAuth Client ID
export const GOOGLE_CLIENT_ID = process.env.REACT_APP_GOOGLE_CLIENT_ID || '';

// Configurações de API
export const API_TIMEOUT = 30000; // 30 segundos
export const API_RETRY_ATTEMPTS = 3;
export const API_RETRY_DELAY = 1000; // 1 segundo

// Headers padrão da API
export const DEFAULT_HEADERS = {
  'Content-Type': 'application/json',
  'Accept': 'application/json'
};

// Configurações de cache
export const CACHE_CONFIG = {
  maxAge: 5 * 60 * 1000, // 5 minutos
  maxItems: 100
};

// Configurações de tema
export const THEME = {
  primary: '#7c3aed',
  secondary: '#6d28d9',
  success: '#10b981',
  warning: '#f59e0b',
  error: '#ef4444',
  background: '#f8fafc',
  text: '#1f2937',
  border: '#e5e7eb',
  muted: '#6b7280'
};

// Configurações de paginação
export const PAGINATION = {
  defaultPageSize: 20,
  maxPageSize: 100
};

// Configurações de upload
export const UPLOAD = {
  maxFileSize: 15 * 1024 * 1024, // 15MB
  allowedTypes: ['image/jpeg', 'image/png', 'image/gif', 'application/pdf'],
  maxFiles: 10
};

// Configurações de validação
export const VALIDATION = {
  password: {
    minLength: 8,
    requireUppercase: true,
    requireLowercase: true,
    requireNumbers: true,
    requireSpecialChars: false
  },
  username: {
    minLength: 3,
    maxLength: 50,
    pattern: /^[a-zA-Z0-9_]+$/
  },
  email: {
    pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  }
};

// Configurações de notificações
export const NOTIFICATIONS = {
  defaultDuration: 5000,
  position: 'top-right',
  autoClose: true
};

// Configurações de gráficos
export const CHARTS = {
  colors: [
    '#7c3aed',
    '#6366f1',
    '#8b5cf6',
    '#a855f7',
    '#c084fc',
    '#d8b4fe',
    '#e9d5ff',
    '#f3e8ff'
  ],
  defaultHeight: 400,
  responsive: true
};

// Configurações de segurança
export const SECURITY = {
  tokenKey: 'token',
  refreshTokenKey: 'refreshToken',
  sessionTimeout: 30 * 60 * 1000, // 30 minutos
  maxLoginAttempts: 5
};

// Configurações de desenvolvimento
export const DEV_CONFIG = {
  enableLogs: process.env.NODE_ENV === 'development',
  enableDebugMode: process.env.NODE_ENV === 'development',
  mockApi: process.env.REACT_APP_MOCK_API === 'true'
};
