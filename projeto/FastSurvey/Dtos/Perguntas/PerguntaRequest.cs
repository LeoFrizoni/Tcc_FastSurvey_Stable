#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FASTSURVEY.Dtos.Opcoes;

namespace FASTSURVEY.Dtos.Perguntas
{
    public enum TipoPerguntaDto
    {
        Discursiva = 1,
        Objetiva = 2,
        MultiplaEscolha = 3,
    }

    public abstract class PerguntaDtoBase
    {
        [Required]
        public int PesquisaId { get; set; }

        [Required]
        public TipoPerguntaDto Tipo { get; set; }

        [Required, MinLength(1)]
        public string Texto { get; set; } = string.Empty;
        public bool TemGabarito { get; set; } = false;
        public int Ordem { get; set; } = 0;

        // Comuns
        public bool Obrigatoria { get; set; } = true;
        public string? Instrucoes { get; set; }
        public bool PermiteAnexo { get; set; } = false;
        public string? TiposAnexoPermitidos { get; set; }
        public int? TamanhoMaximoAnexo { get; set; }
        public int? LimiteCaracteres { get; set; }
        public int? TempoLimite { get; set; }
        public string? ConfiguracaoJson { get; set; }

        // Opções
        public List<OpcaoPerguntaRequest>? Opcoes { get; set; }

        // Regras específicas
        public bool? PermiteMultiplasSelecao { get; set; }
        public bool? PermiteOutraOpcao { get; set; }
        public bool? MostrarNumeroRespostas { get; set; }
        public bool? OrdenarAleatoriamente { get; set; }

        // Gabaritos
        public int? OpcaoCorretaId { get; set; }
        public List<int>? OpcoesCorretasIds { get; set; }
    }

    public sealed class CriarPerguntaRequest : PerguntaDtoBase { }

    public abstract class AtualizarPerguntaBase : PerguntaDtoBase
    {
        [Required]
        public int PerguntaId { get; set; }
    }

    public sealed class AtualizarPerguntaRequest : AtualizarPerguntaBase { }

    public sealed class ReordenarPerguntasRequest
    {
        [Required]
        public int PesquisaId { get; set; }

        [Required]
        public List<ReordenarItem> Perguntas { get; set; } = new();
    }

    public sealed class ReordenarItem
    {
        [Required]
        public int PerguntaId { get; set; }
        public int NovaOrdem { get; set; }
    }
}
