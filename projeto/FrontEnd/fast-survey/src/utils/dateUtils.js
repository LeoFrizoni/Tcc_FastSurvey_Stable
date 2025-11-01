/**
 * Utilitários para formatação de datas
 */

/**
 * Formata uma data para o padrão brasileiro DD/MM/YYYY
 * @param {string|Date|number} data - Data a ser formatada
 * @returns {string} Data formatada como DD/MM/YYYY ou "—" se inválida
 */
export function formatarDataBR(data) {
  if (!data) return "—";
  
  try {
    let date;
    
    // Se for string, tentar parsear de forma mais robusta
    if (typeof data === 'string') {
      // Se contém T (ISO format), usar parse direto
      if (data.includes('T')) {
        date = new Date(data);
      } else {
        // Se não contém T, pode ser DD/MM/YYYY ou MM/DD/YYYY
        // Vamos assumir que é ISO ou DD/MM/YYYY
        date = new Date(data);
      }
    } else {
      date = new Date(data);
    }
    
    if (isNaN(date.getTime())) {
      console.warn('Data inválida recebida:', data);
      return "—";
    }
    
    // Formatar como DD/MM/YYYY
    const dia = String(date.getDate()).padStart(2, '0');
    const mes = String(date.getMonth() + 1).padStart(2, '0');
    const ano = date.getFullYear();
    
    return `${dia}/${mes}/${ano}`;
  } catch (error) {
    console.error('Erro ao formatar data:', error, 'Data recebida:', data);
    return "—";
  }
}

/**
 * Formata uma data para o padrão ISO (YYYY-MM-DD)
 * @param {string|Date|number} data - Data a ser formatada
 * @returns {string} Data formatada como YYYY-MM-DD ou string vazia se inválida
 */
export function formatarDataISO(data) {
  if (!data) return "";
  
  try {
    const date = new Date(data);
    
    if (isNaN(date.getTime())) {
      return "";
    }
    
    const ano = date.getFullYear();
    const mes = String(date.getMonth() + 1).padStart(2, '0');
    const dia = String(date.getDate()).padStart(2, '0');
    
    return `${ano}-${mes}-${dia}`;
  } catch (error) {
    console.error('Erro ao formatar data ISO:', error);
    return "";
  }
}

/**
 * Formata uma data para exibição completa (DD/MM/YYYY HH:MM)
 * @param {string|Date|number} data - Data a ser formatada
 * @returns {string} Data formatada como DD/MM/YYYY HH:MM ou "—" se inválida
 */
export function formatarDataCompleta(data) {
  if (!data) return "—";
  
  try {
    const date = new Date(data);
    
    if (isNaN(date.getTime())) {
      return "—";
    }
    
    const dia = String(date.getDate()).padStart(2, '0');
    const mes = String(date.getMonth() + 1).padStart(2, '0');
    const ano = date.getFullYear();
    const horas = String(date.getHours()).padStart(2, '0');
    const minutos = String(date.getMinutes()).padStart(2, '0');
    
    return `${dia}/${mes}/${ano} ${horas}:${minutos}`;
  } catch (error) {
    console.error('Erro ao formatar data completa:', error);
    return "—";
  }
}
