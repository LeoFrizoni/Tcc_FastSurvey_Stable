// Configuração de ambiente forçada
// Este arquivo garante que as variáveis de ambiente sejam carregadas corretamente

// TESTE DIRETO - Verificar variáveis no momento da importação
console.log('🔧 TESTE DIRETO - Variáveis no momento da importação:', {
  NODE_ENV: process.env.NODE_ENV,
  REACT_APP_GOOGLE_CLIENT_ID: process.env.REACT_APP_GOOGLE_CLIENT_ID,
  REACT_APP_API_URL: process.env.REACT_APP_API_URL,
  REACT_APP_ENV: process.env.REACT_APP_ENV,
  allProcessEnv: Object.keys(process.env || {}),
  reactAppKeys: Object.keys(process.env || {}).filter(key => key.startsWith('REACT_APP_'))
});

// Carregar variáveis de ambiente manualmente se necessário
const loadEnvVars = () => {
  // Verificar se estamos no browser
  if (typeof window !== 'undefined') {
    // No browser, as variáveis devem estar disponíveis via process.env
    return {
      NODE_ENV: process.env.NODE_ENV || 'development',
      REACT_APP_GOOGLE_CLIENT_ID: process.env.REACT_APP_GOOGLE_CLIENT_ID || '',
      REACT_APP_API_URL: process.env.REACT_APP_API_URL || 'http://localhost:5062',
      REACT_APP_ENV: process.env.REACT_APP_ENV || 'development'
    };
  }
  
  // No servidor, usar process.env diretamente
  return {
    NODE_ENV: process.env.NODE_ENV || 'development',
    REACT_APP_GOOGLE_CLIENT_ID: process.env.REACT_APP_GOOGLE_CLIENT_ID || '',
    REACT_APP_API_URL: process.env.REACT_APP_API_URL || 'http://localhost:5062',
    REACT_APP_ENV: process.env.REACT_APP_ENV || 'development'
  };
};

// Carregar variáveis
const envVars = loadEnvVars();

// Debug detalhado da variável Google Client ID
console.log('🔧 DEBUG DETALHADO - Google Client ID:', {
  rawValue: process.env.REACT_APP_GOOGLE_CLIENT_ID,
  processedValue: envVars.REACT_APP_GOOGLE_CLIENT_ID,
  rawValueType: typeof process.env.REACT_APP_GOOGLE_CLIENT_ID,
  processedValueType: typeof envVars.REACT_APP_GOOGLE_CLIENT_ID,
  rawValueLength: process.env.REACT_APP_GOOGLE_CLIENT_ID?.length || 0,
  processedValueLength: envVars.REACT_APP_GOOGLE_CLIENT_ID?.length || 0,
  isEmpty: !process.env.REACT_APP_GOOGLE_CLIENT_ID,
  isUndefined: process.env.REACT_APP_GOOGLE_CLIENT_ID === undefined,
  isNull: process.env.REACT_APP_GOOGLE_CLIENT_ID === null,
  isString: typeof process.env.REACT_APP_GOOGLE_CLIENT_ID === 'string'
});

// Debug: Log das variáveis carregadas
console.log('🔧 ENV CONFIG - Variáveis carregadas:', {
  NODE_ENV: envVars.NODE_ENV,
  REACT_APP_GOOGLE_CLIENT_ID: envVars.REACT_APP_GOOGLE_CLIENT_ID ? 'CONFIGURADO' : 'NÃO CONFIGURADO',
  REACT_APP_API_URL: envVars.REACT_APP_API_URL,
  REACT_APP_ENV: envVars.REACT_APP_ENV,
  hasGoogleClientId: !!envVars.REACT_APP_GOOGLE_CLIENT_ID,
  hasApiUrl: !!envVars.REACT_APP_API_URL
});

export default envVars;
