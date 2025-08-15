using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Services
{
    public class ServicePerguntas
    {
        private FastSurveyContext _context;

        private RepositoryPerguntas _RepositoryPerguntas { get; set; }
        private RepositoryTipoPergunta _RepositoryTipoPergunta { get; set; }
        private RepositoryRespostas _RepositoryRespostas { get; set; }

        public ServicePerguntas(FastSurveyContext context)
        {
            _context = context;
            _RepositoryPerguntas = new RepositoryPerguntas(_context, true);
            _RepositoryTipoPergunta = new RepositoryTipoPergunta(_context, true);
            _RepositoryRespostas = new RepositoryRespostas(_context, true);

        }

        public async Task<List<perguntas>> ListarTodasPerguntasAsync()
        {
            return await _RepositoryPerguntas.SelecionarTodosAsync();
        }

        public async Task<perguntas> BuscarPerguntaPorIdAsync(int id)
        {
            return await _RepositoryPerguntas.SelecionarChaveAsync(id);
        }

        public async Task<perguntas> CadastrarPerguntaAsync(perguntas pergunta)
        {
            return await _RepositoryPerguntas.IncluirAsync(pergunta);
        }

        public async Task<perguntas> AtualizarPerguntaAsync(perguntas pergunta)
        {
            return await _RepositoryPerguntas.AlterarAsync(pergunta);
        }

        public async Task ExcluirPerguntaAsync(int id)
        {
            var pergunta = await _RepositoryPerguntas.SelecionarChaveAsync(id);
            if (pergunta != null)
            {
                await _RepositoryPerguntas.ExcluirAsync(pergunta);
            }
        }
    }
}
