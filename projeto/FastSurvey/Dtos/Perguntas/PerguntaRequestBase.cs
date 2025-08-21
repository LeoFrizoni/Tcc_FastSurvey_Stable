// FASTSURVEY/Dtos/Perguntas/Base/PerguntaBaseRequests.cs
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Perguntas.Base
{
    public abstract class PerguntaCreateRequestBase
    {
        [Required] public int PesquisaId { get; set; }
        [Required, StringLength(280)] public string Texto { get; set; } = "";
        public bool Obrigatoria { get; set; } = false;
        public int Ordem { get; set; } = 0;
    }

    public abstract class PerguntaUpdateRequestBase
    {
        [Required] public int PerguntaId { get; set; }
        [Required] public int PesquisaId { get; set; }
        [Required, StringLength(280)] public string Texto { get; set; } = "";
        public bool Obrigatoria { get; set; } = false;
        public int Ordem { get; set; } = 0;
    }
}
