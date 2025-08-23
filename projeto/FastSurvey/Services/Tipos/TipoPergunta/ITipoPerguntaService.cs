using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Tipos
{
    public interface ITipoPerguntaService
    {
        /// <summary>
        /// Lista tipos de pergunta. Quando incluirDesabilitados=false, retorna apenas ativos.
        /// </summary>
        Task<List<Tipopergunta>> ListarAsync(bool incluirDesabilitados = false, CancellationToken ct = default);
    }
}
