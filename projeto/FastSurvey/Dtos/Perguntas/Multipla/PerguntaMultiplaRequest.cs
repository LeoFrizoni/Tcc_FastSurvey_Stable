// FASTSURVEY/Dtos/Perguntas/Multipla/PerguntaMultiplaRequests.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Dtos.Perguntas.Base;

namespace FASTSURVEY.Dtos.Perguntas.Multipla
{
    public class CriarPerguntaMultiplaRequest : PerguntaCreateRequestBase
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new();

        // Regras
        public bool TemGabarito { get; set; } = false;
        public bool PermiteMultiplaSelecao { get; set; } = true; // coerente com Model
        public List<int>? OpcoesCorretasIds { get; set; } // se TemGabarito = true
    }

    public class AtualizarPerguntaMultiplaRequest : PerguntaUpdateRequestBase
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new();
        public bool TemGabarito { get; set; } = false;
        public bool PermiteMultiplaSelecao { get; set; } = true;
        public List<int>? OpcoesCorretasIds { get; set; }
    }
}
