import React from 'react';
import AppRoutes from './routes/routes';
import './config/axios'; // Importar axios configurado globalmente

// TESTE - Verificar variáveis de ambiente no App.js
console.log('🔧 TESTE APP.JS - Variáveis de ambiente:', {
  NODE_ENV: process.env.NODE_ENV,
  REACT_APP_GOOGLE_CLIENT_ID: process.env.REACT_APP_GOOGLE_CLIENT_ID,
  REACT_APP_API_URL: process.env.REACT_APP_API_URL,
  REACT_APP_ENV: process.env.REACT_APP_ENV,
  allEnvVars: Object.keys(process.env).filter(key => key.startsWith('REACT_APP_'))
});

export default function App() {
  return <AppRoutes />;
}
