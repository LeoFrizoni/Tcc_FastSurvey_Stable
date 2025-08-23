// FASTSURVEY/Dtos/Perguntas/Base/PerguntaBaseRequests.cs
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Perguntas
{
    public class PerguntaRequestBase
    {
        public int PesquisaId { get; set; }
        public int TipoPerguntaId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public bool TemGabarito { get; set; } = false;
        public int Ordem { get; set; } = 0;
    }

    public class CriarPerguntaRequest : PerguntaRequestBase
    {
        public bool PermiteMultiplaSelecao { get; set; } = false;
    }

    public class AtualizarPerguntaRequestBase : PerguntaRequestBase
    {
        public int PerguntaId { get; set; }
        public new int Ordem { get; set; } = 0;
    }
}
