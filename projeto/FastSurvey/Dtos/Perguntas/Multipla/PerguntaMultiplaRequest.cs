// FASTSURVEY/Dtos/Perguntas/Multipla/PerguntaMultiplaRequests.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FASTSURVEY.Dtos.Opcoes;
namespace FASTSURVEY.Dtos.Perguntas.Multipla
{
    public class CriarPerguntaMultiplaRequest : FASTSURVEY.Dtos.Perguntas.CriarPerguntaRequest
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new();

        // Regras
        public new bool TemGabarito { get; set; } = false;
        public new bool PermiteMultiplaSelecao { get; set; } = true; // coerente com Model
        public List<int>? OpcoesCorretasIds { get; set; } // se TemGabarito = true
    }

    public class AtualizarPerguntaMultiplaRequest : FASTSURVEY.Dtos.Perguntas.AtualizarPerguntaRequestBase
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new();
        public new bool TemGabarito { get; set; } = false;
        public new bool PermiteMultiplaSelecao { get; set; } = true;
        public List<int>? OpcoesCorretasIds { get; set; }
    }
}
