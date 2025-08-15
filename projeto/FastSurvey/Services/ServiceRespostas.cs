using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FASTSURVEY.Services
{
    public class ServiceRespostas
    {
        private readonly RepositoryRespostas _repositoryRespostas;

        public ServiceRespostas(RepositoryRespostas repositoryRespostas)
        {
            _repositoryRespostas = repositoryRespostas;
        }

        public async Task<List<respostas>> ListarTodasRespostasAsync()
        {
            return await _repositoryRespostas.SelecionarTodosAsync();
        }

        public async Task<respostas> ObterRespostaPorIdAsync(int id)
        {
            return await _repositoryRespostas.SelecionarChaveAsync(id);
        }

        public async Task<respostas> CadastrarRespostaAsync(respostas resposta)
        {
            if (resposta == null)
                throw new ArgumentNullException(nameof(resposta), "Resposta não pode ser nula.");

            return await _repositoryRespostas.IncluirAsync(resposta);
        }

        public async Task<respostas> AtualizarRespostaAsync(respostas resposta)
        {
            if (resposta == null)
                throw new ArgumentNullException(nameof(resposta), "Resposta não pode ser nula.");

            var respostaExistente = await _repositoryRespostas.SelecionarChaveAsync(resposta.respostaid);
            if (respostaExistente == null)
                throw new KeyNotFoundException($"Resposta com ID {resposta.respostaid} não encontrada.");

            return await _repositoryRespostas.AlterarAsync(resposta);
        }

        public async Task<bool> ExcluirRespostaAsync(int id)
        {
            var resposta = await _repositoryRespostas.SelecionarChaveAsync(id);
            if (resposta == null)
                throw new KeyNotFoundException($"Resposta com ID {id} não encontrada.");

            await _repositoryRespostas.ExcluirAsync(resposta);
            return true;
        }
    }
}
