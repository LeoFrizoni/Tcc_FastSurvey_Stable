// FASTSURVEY/Services/Tipos/TipoPerguntaService.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Tipos;
using SISTEMA_FASTSURVEY.MODEL.Interfaces; // ITipoPerguntaRepository
using SISTEMA_FASTSURVEY.MODEL.Models; // TipoPergunta (entidade)

namespace FASTSURVEY.Services.Tipos
{
    public class TipoPerguntaService : ITipoPerguntaService
    {
        private readonly ITipoPerguntaRepository _repo;

        public TipoPerguntaService(ITipoPerguntaRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<TipoPerguntaCatalogDto>> ListarAsync(
            bool incluirDesabilitados = false,
            CancellationToken ct = default
        )
        {
            IEnumerable<TipoPergunta> fonte;

            if (incluirDesabilitados)
                fonte = await _repo.ListAsync(ct: ct);
            else
                fonte = await _repo.ListarAtivosAsync(ct);

            var lista = fonte
                .OrderBy(x => x.TipoPergunta1 ?? string.Empty)
                .Select(x => new TipoPerguntaCatalogDto
                {
                    TipoPerguntaId = x.TipoPerguntaId,
                    TipoPergunta = x.TipoPergunta1 ?? string.Empty,
                    Desabilitado = x.Desabilitado,
                })
                .ToList();

            return lista;
        }
    }
}
