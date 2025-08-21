// FASTSURVEY/Dtos/Perguntas/Objetiva/PerguntaObjetivaRequests.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Dtos.Perguntas.Base;

namespace FASTSURVEY.Dtos.Perguntas.Objetiva
{
    public class CriarPerguntaObjetivaRequest : PerguntaCreateRequestBase
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new List<OpcaoPerguntaRequest>();
        public bool TemGabarito { get; set; } = false;
        public int? OpcaoCorretaId { get; set; } // se TemGabarito = true
    }

    public class AtualizarPerguntaObjetivaRequest : PerguntaUpdateRequestBase
    {
        [Required] public List<OpcaoPerguntaRequest> Opcoes { get; set; } = new List<OpcaoPerguntaRequest>();
        public bool TemGabarito { get; set; } = false;
        public int? OpcaoCorretaId { get; set; }
    }
}
