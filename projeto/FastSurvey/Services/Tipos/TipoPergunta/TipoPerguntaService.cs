using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Services.Tipos
{
    public class TipoPerguntaService : ITipoPerguntaService
    {
        private readonly ITipoPerguntaRepository _repo;

        public TipoPerguntaService(ITipoPerguntaRepository repo) => _repo = repo;

        public async Task<List<Tipopergunta>> ListarAsync(bool incluirDesabilitados = false, CancellationToken ct = default)
        {
            if (incluirDesabilitados)
            {
                var todos = await _repo.ListAsync(ct: ct); // Corrigido para usar ListAsync
                return todos.OrderBy(x => x.Tipopergunta1 ?? string.Empty).ToList();
            }

            return await _repo.ListarAtivosAsync(ct);
        }
    }
}
