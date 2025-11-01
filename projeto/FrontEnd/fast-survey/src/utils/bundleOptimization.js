// src/utils/bundleOptimization.js
export const preloadComponent = (importFunc) => {
  const componentImport = importFunc();
  return componentImport;
};

export const loadComponentOnDemand = (importFunc) => {
  return () => importFunc();
};

// Pre-carregamento inteligente de rotas críticas
export const preloadCriticalRoutes = () => {
  // Preload apenas as rotas mais usadas após 2 segundos
  setTimeout(() => {
    import('../pages/home/home');
    import('../pages/createPesquisa/createPesquisa');
    import('../pages/responderPesquisa/responder-pesquisa');
  }, 2000);
};

// Função para detectar se o usuário está em uma conexão lenta
export const isSlowConnection = () => {
  if ('connection' in navigator) {
    const connection = navigator.connection;
    return connection.effectiveType === 'slow-2g' || connection.effectiveType === '2g';
  }
  return false;
};
