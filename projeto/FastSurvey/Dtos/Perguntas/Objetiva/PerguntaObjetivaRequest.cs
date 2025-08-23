// FASTSURVEY/Dtos/Perguntas/Objetiva/PerguntaObjetivaRequests.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FASTSURVEY.Dtos.Opcoes;
namespace FASTSURVEY.Dtos.Perguntas.Objetiva
{
    public class CriarPerguntaObjetivaRequest : FASTSURVEY.Dtos.Perguntas.CriarPerguntaRequest
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new List<OpcaoPerguntaRequest>();
        public new bool TemGabarito { get; set; } = false;
        public int? OpcaoCorretaId { get; set; } // se TemGabarito = true
    }

    public class AtualizarPerguntaObjetivaRequest : FASTSURVEY.Dtos.Perguntas.AtualizarPerguntaRequestBase
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new List<OpcaoPerguntaRequest>();
        public new bool TemGabarito { get; set; } = false;
        public int? OpcaoCorretaId { get; set; }
    }
}
