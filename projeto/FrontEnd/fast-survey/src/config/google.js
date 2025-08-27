// src/config/google.js
// Google Identity Services (OAuth 2.0) — não é Google+.
// Pré-requisitos:
// 1) Projeto no Google Cloud Console
// 2) OAuth 2.0 Client ID (tipo Web)
// 3) Authorized JavaScript origins: http://localhost:3000 (dev) e seu domínio de produção
// 4) Em React (CRA), usar REACT_APP_GOOGLE_CLIENT_ID no .env.local

import env from './env';

const isDev = env.NODE_ENV === 'development';

export const GOOGLE_CONFIG = {
  CLIENT_ID: env.REACT_APP_GOOGLE_CLIENT_ID || '',
  AUTHORIZED_ORIGINS: [
    'http://localhost:3000',
    'https://your-production-domain.com',
  ],
  BUTTON_CONFIG: {
    theme: 'outline',
    size: 'large',
    type: 'standard',
    text: 'signin_with',
    shape: 'pill',
    width: 280,
    logoAlignment: 'left',
  },
};

export const isGoogleConfigured = () => Boolean(GOOGLE_CONFIG.CLIENT_ID);

export const getGoogleConfigError = () => {
  if (!isGoogleConfigured()) {
    return {
      title: 'Google Sign-In não configurado',
      message:
        'Configure REACT_APP_GOOGLE_CLIENT_ID no arquivo .env.local (Google Identity Services).',
      instructions: [
        '1) Crie um OAuth 2.0 Client ID (Web) no Google Cloud Console',
        '2) Em Authorized JavaScript origins, adicione http://localhost:3000 e o domínio de produção',
        '3) Copie o Client ID para REACT_APP_GOOGLE_CLIENT_ID no .env.local',
        '4) Reinicie o servidor de desenvolvimento do React',
      ],
    };
  }
  return null;
};

if (isDev) {
  const masked = (v) => (v ? v.slice(0, 10) + '…' : 'N/A');
  // eslint-disable-next-line no-console
  console.log('Google (dev):', {
    hasClientId: isGoogleConfigured(),
    clientId: masked(GOOGLE_CONFIG.CLIENT_ID),
  });
}
