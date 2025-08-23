// Configurações da aplicação
export const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5062';

// Google OAuth Client ID (substitua pelo seu ID real)
export const GOOGLE_CLIENT_ID = process.env.REACT_APP_GOOGLE_CLIENT_ID || '';

// Configurações de tema
export const THEME = {
  primary: '#667eea',
  secondary: '#764ba2',
  success: '#10b981',
  warning: '#f59e0b',
  error: '#ef4444',
  background: '#f8fafc',
  text: '#1f2937'
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
