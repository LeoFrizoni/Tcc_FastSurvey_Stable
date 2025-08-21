// FASTSURVEY/Dtos/Perguntas/Base/PerguntaResponses.cs
using FASTSURVEY.Dtos.Perguntas.Base;
using FASTSURVEY.Dtos.Opcoes;
using System.Collections.Generic;

namespace FASTSURVEY.Dtos.Perguntas
{
    public class PerguntaResponse
    {
        public int PerguntaId { get; set; }
        public int PesquisaId { get; set; }
        public TipoPerguntaDto Tipo { get; set; }
        public string Texto { get; set; } = "";
        public bool Obrigatoria { get; set; }
        public int Ordem { get; set; }

        // Propriedades específicas (preenchidas conforme tipo)
        public bool TemGabarito { get; set; }
        public bool PermiteMultiplaSelecao { get; set; }
        public List<OpcaoPerguntaResponse>? Opcoes { get; set; }
        public List<int>? OpcoesCorretasIds { get; set; }
        public int? OpcaoCorretaId { get; set; }
    }
}
