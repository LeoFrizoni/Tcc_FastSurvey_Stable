export function handleApiError(error) {
  if (error.response) {
    // Erro vindo da API (status 4xx ou 5xx)
    console.error('Erro da API:', error.response.data);
    return error.response.data?.message || 'Erro desconhecido da API.';
  } else if (error.request) {
    // Sem resposta
    console.error('Sem resposta da API:', error.request);
    return 'Sem resposta do servidor.';
  } else {
    // Outro tipo de erro
    console.error('Erro geral:', error.message);
    return 'Erro inesperado.';
  }
}
