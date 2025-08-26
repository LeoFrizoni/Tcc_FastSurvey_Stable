#nullable enable
using System;
using System.Collections.Generic;
using FASTSURVEY.Dtos.Opcoes;

namespace FASTSURVEY.Dtos.Perguntas
{
    public class PerguntaResponseBase
    {
        public int PerguntaId { get; set; }
        public TipoPerguntaDto Tipo { get; set; }
        public int PesquisaId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public bool TemGabarito { get; set; }
        public bool PermiteMultiplasSelecao { get; set; }
        public int Ordem { get; set; }

        // Comuns
        public bool Obrigatoria { get; set; }
        public string? Instrucoes { get; set; }
        public bool PermiteAnexo { get; set; }
        public string? TiposAnexoPermitidos { get; set; }
        public int? TamanhoMaximoAnexo { get; set; }
        public int? LimiteCaracteres { get; set; }
        public int? TempoLimite { get; set; }
        public string? ConfiguracaoJson { get; set; }
    }

    public class PerguntaResponse : PerguntaResponseBase
    {
        public IReadOnlyList<OpcaoPerguntaResponse> Opcoes { get; set; } =
            Array.Empty<OpcaoPerguntaResponse>();

        // Gabaritos
        public int? OpcaoCorretaId { get; set; }
        public IReadOnlyList<int>? OpcoesCorretasIds { get; set; }
    }
}
