// FASTSURVEY/Dtos/Perguntas/Discursiva/PerguntaDiscursivaRequests.cs
using System.ComponentModel.DataAnnotations;
using FASTSURVEY.Dtos.Perguntas.Base;

namespace FASTSURVEY.Dtos.Perguntas.Discursiva
{
    public class CriarPerguntaDiscursivaRequest : PerguntaCreateRequestBase
    {
        // Sem campos extras, pois a Model Pergunta não tem colunas específicas
    }

    public class AtualizarPerguntaDiscursivaRequest : PerguntaUpdateRequestBase
    {
        // Idem
    }
}
