#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using PerguntaEntity = SISTEMA_FASTSURVEY.MODEL.Models.Perguntas;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IPerguntaRepository : IRepository<PerguntaEntity>
    {
        /// <summary>
        /// Lista perguntas de uma pesquisa. Por padrão inclui Opções e Anexos.
        /// NÃO requer alterações na database.
        /// </summary>
        Task<List<PerguntaEntity>> ListarPorPesquisaAsync(
            int pesquisaId,
            bool incluirOpcoes = true,
            bool incluirAnexos = true,
            bool incluirTipoPergunta = false,
            bool incluirRespostas = false,
            bool asNoTracking = true,
            CancellationToken ct = default
        );

        /// <summary>
        /// Versão leve para telas/listas (projeção DTO).
        /// </summary>
        Task<List<PerguntaResumoDto>> ListarResumoPorPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
    }

    public sealed class PerguntaResumoDto
    {
        public int PerguntaId { get; init; }
        public string Texto { get; init; } = string.Empty;
        public int Ordem { get; init; }
        public int TipoPerguntaId { get; init; }
        public bool TemGabarito { get; init; }
        public bool PermiteMultiplasSelecao { get; init; }
        public int QtdeOpcoes { get; init; }
        public int QtdeAnexos { get; init; }
    }
}
