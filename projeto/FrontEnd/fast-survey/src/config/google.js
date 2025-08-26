// Configuração do Google OAuth
// Para usar o Google Sign-In, você precisa:
// 1. Criar um projeto no Google Cloud Console
// 2. Habilitar a API do Google+ 
// 3. Criar credenciais OAuth 2.0
// 4. Adicionar os domínios autorizados (localhost:3000 para desenvolvimento)
// 5. Configurar REACT_APP_GOOGLE_CLIENT_ID no arquivo .env
// Importar configuração de ambiente
import envVars from './env.js';
// TESTE DIRETO - Verificar variáveis diretamente
console.log('🔧 TESTE DIRETO GOOGLE.JS - Variáveis de ambiente:', {
  NODE_ENV: process.env.NODE_ENV,
  REACT_APP_GOOGLE_CLIENT_ID: process.env.REACT_APP_GOOGLE_CLIENT_ID,
  REACT_APP_API_URL: process.env.REACT_APP_API_URL,
  REACT_APP_ENV: process.env.REACT_APP_ENV,
  allProcessEnv: Object.keys(process.env || {}),
  reactAppKeys: Object.keys(process.env || {}).filter(key => key.startsWith('REACT_APP_'))
});



// DIAGNÓSTICO COMPLETO - Verificar configuração do React
console.log('🔍 DIAGNÓSTICO REACT - Configuração de Ambiente:', {
  // Ambiente atual
  NODE_ENV: envVars.NODE_ENV,
  isDevelopment: envVars.NODE_ENV === 'development',
  isProduction: envVars.NODE_ENV === 'production',
  
  // Verificar se o React está carregando variáveis
  hasProcessEnv: typeof process !== 'undefined' && typeof process.env !== 'undefined',
  processEnvKeys: Object.keys(process.env || {}),
  
  // Todas as variáveis REACT_APP
  reactAppVars: Object.keys(process.env || {}).filter(key => key.startsWith('REACT_APP_')),
  
  // Variáveis específicas
  googleClientId: envVars.REACT_APP_GOOGLE_CLIENT_ID,
  apiUrl: envVars.REACT_APP_API_URL,
  env: envVars.REACT_APP_ENV,
  
  // Verificar se as variáveis estão sendo lidas
  googleClientIdExists: !!envVars.REACT_APP_GOOGLE_CLIENT_ID,
  apiUrlExists: !!envVars.REACT_APP_API_URL,
  envExists: !!envVars.REACT_APP_ENV
});

export const GOOGLE_CONFIG = {
  // Client ID deve ser configurado via variável de ambiente
  CLIENT_ID: envVars.REACT_APP_GOOGLE_CLIENT_ID || '',
  
  // URLs autorizadas
  AUTHORIZED_ORIGINS: [
    'http://localhost:3000',
    'http://localhost:3001',
    'http://localhost:5062',
    'https://your-production-domain.com'
  ],
  
  // Configurações do botão
  BUTTON_CONFIG: {
    theme: 'outline',
    size: 'large',
    type: 'standard',
    text: 'signin_with',
    shape: 'pill',
    width: 280,
    logoAlignment: 'left'
  }
};

// Verifica se o Google Client ID está configurado
export const isGoogleConfigured = () => {
  const isConfigured = GOOGLE_CONFIG.CLIENT_ID && 
         GOOGLE_CONFIG.CLIENT_ID !== '' &&
         GOOGLE_CONFIG.CLIENT_ID !== 'seu-google-client-id-aqui';
  
  console.log('🔍 DEBUG - isGoogleConfigured:', {
    clientId: GOOGLE_CONFIG.CLIENT_ID,
    isConfigured: isConfigured,
    clientIdLength: GOOGLE_CONFIG.CLIENT_ID?.length || 0
  });
  
  return isConfigured;
};

// Mensagem de erro para configuração
export const getGoogleConfigError = () => {
  if (!isGoogleConfigured()) {
    return {
      title: 'Google Sign-In não configurado',
      message: 'Para usar o login com Google, configure o REACT_APP_GOOGLE_CLIENT_ID no arquivo .env',
      instructions: [
        '1. Crie um projeto no Google Cloud Console',
        '2. Habilite a API do Google+',
        '3. Crie credenciais OAuth 2.0',
        '4. Adicione localhost:3000 aos domínios autorizados',
        '5. Configure REACT_APP_GOOGLE_CLIENT_ID no arquivo .env',
        '6. Copie o arquivo env.example para .env e configure as variáveis'
      ]
    };
  }
  return null;
};
