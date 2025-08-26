#nullable enable
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IRespostaRepository : IRepository<Respostas>
    {
        /// <summary>
        /// Lista respostas de uma pergunta. Por padrão inclui Opções e Anexos.
        /// </summary>
        Task<List<Respostas>> ListarPorPerguntaAsync(
            int perguntaId,
            bool incluirOpcoes = true,
            bool incluirAnexos = true,
            int skip = 0,
            int take = 200,
            CancellationToken ct = default
        );

        /// <summary>
        /// Lista respostas filtrando por Pesquisa (via navegação Pergunta->Pesquisa).
        /// </summary>
        Task<List<Respostas>> ListarPorPesquisaAsync(
            int pesquisaId,
            bool incluirOpcoes = false,
            bool incluirAnexos = false,
            int skip = 0,
            int take = 500,
            CancellationToken ct = default
        );

        /// <summary>
        /// Lista respostas de uma sessão (útil para auditoria e debug).
        /// Pode opcionalmente filtrar por Pesquisa.
        /// </summary>
        Task<List<Respostas>> ListarPorSessaoAsync(
            string sessaoId,
            int? pesquisaId = null,
            bool incluirOpcoes = false,
            bool incluirAnexos = false,
            int skip = 0,
            int take = 500,
            CancellationToken ct = default
        );

        /// <summary>Conta respostas de uma pergunta (opcionalmente só não anônimas e/ou por sessão).</summary>
        Task<int> ContarPorPerguntaAsync(
            int perguntaId,
            bool somenteNaoAnonimas = false,
            string? sessaoId = null,
            CancellationToken ct = default
        );

        /// <summary>Resumo agregador para resultados/analytics rápidos da pergunta.</summary>
        Task<RespostaResumoDto> ResumoDaPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        );

        /// <summary>
        /// Contagem por opção marcada (para perguntas de opção única/múltipla).
        /// Atenção: utiliza a coleção Respostas.Opcao (N:N).
        /// </summary>
        Task<List<OpcaoContagemDto>> ContagemPorOpcaoAsync(
            int perguntaId,
            CancellationToken ct = default
        );

        /// <summary>Somente respostas textuais não vazias de uma pergunta.</summary>
        Task<List<RespostaTextoDto>> ListarTextosPorPerguntaAsync(
            int perguntaId,
            int skip = 0,
            int take = 500,
            CancellationToken ct = default
        );

        /// <summary>Remove em massa todas as respostas de uma pergunta.</summary>
        Task<int> RemoverPorPerguntaAsync(int perguntaId, CancellationToken ct = default);

        /// <summary>Remove em massa todas as respostas de uma sessão (opcionalmente filtradas por Pesquisa).</summary>
        Task<int> RemoverPorSessaoAsync(
            string sessaoId,
            int? pesquisaId = null,
            CancellationToken ct = default
        );
    }

    // ==== DTOs leves para resultados ====

    public sealed class RespostaResumoDto
    {
        public int PerguntaId { get; init; }
        public int Total { get; init; }
        public int TotalAnonimas { get; init; }
        public int TotalComTexto { get; init; }
        public int TotalComAnexo { get; init; }
        public int TotalComOpcao { get; init; }
        public DateTime? PrimeiroRegistro { get; init; }
        public DateTime? UltimoRegistro { get; init; }
    }

    public sealed class OpcaoContagemDto
    {
        public int OpcaoId { get; init; }
        public int Qtd { get; init; }
    }

    public sealed class RespostaTextoDto
    {
        public int RespostaId { get; init; }
        public int PerguntaId { get; init; }
        public DateTime DataResposta { get; init; }
        public string? SessaoId { get; init; }
        public bool RespostaAnonima { get; init; }
        public string Texto { get; init; } = "";
    }
}
