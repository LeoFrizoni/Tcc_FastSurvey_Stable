import { useEffect } from 'react';

/**
 * Hook para gerenciar o evento beforeunload
 * Limpa sessionStorage se o usuário não marcou "Lembrar de mim"
 */
export const useBeforeUnload = (lembrarDeMim = false) => {
  useEffect(() => {
    const handleBeforeUnload = () => {
      // Se o usuário está logado mas "Lembrar de mim" não está marcado, limpar sessionStorage
      const token = sessionStorage.getItem('token');
      if (token && !lembrarDeMim) {
        sessionStorage.removeItem('token');
        sessionStorage.removeItem('userId');
        sessionStorage.removeItem('tipousuarioid');
      }
    };

    // Só adiciona o evento se o usuário está logado via sessionStorage
    const token = sessionStorage.getItem('token');
    if (token) {
      window.addEventListener('beforeunload', handleBeforeUnload);
      
      return () => {
        window.removeEventListener('beforeunload', handleBeforeUnload);
      };
    }
  }, [lembrarDeMim]);
};

