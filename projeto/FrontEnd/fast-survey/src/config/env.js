// src/config/env.js
const isDev = process.env.NODE_ENV === 'development';

const env = {
  NODE_ENV: process.env.NODE_ENV ?? 'development',
  REACT_APP_ENV: process.env.REACT_APP_ENV ?? process.env.NODE_ENV ?? 'development',
  REACT_APP_API_URL: process.env.REACT_APP_API_URL ?? 'http://localhost:5062',
  REACT_APP_GOOGLE_CLIENT_ID: process.env.REACT_APP_GOOGLE_CLIENT_ID ?? '',
};

// Logs só no dev e sem expor o Client ID por completo
if (isDev) {
  const masked = (v) => (v ? v.slice(0, 10) + '…' : 'N/A');
  // eslint-disable-next-line no-console
  console.log('ENV (dev):', {
    NODE_ENV: env.NODE_ENV,
    REACT_APP_ENV: env.REACT_APP_ENV,
    REACT_APP_API_URL: env.REACT_APP_API_URL,
    REACT_APP_GOOGLE_CLIENT_ID: masked(env.REACT_APP_GOOGLE_CLIENT_ID),
  });
}

export default env;
