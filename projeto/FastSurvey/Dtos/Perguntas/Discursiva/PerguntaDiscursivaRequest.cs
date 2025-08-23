// FASTSURVEY/Dtos/Perguntas/Discursiva/PerguntaDiscursivaRequests.cs
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Perguntas.Discursiva
{
    public class CriarPerguntaDiscursivaRequest : FASTSURVEY.Dtos.Perguntas.CriarPerguntaRequest
    {
        // Sem campos extras, pois a Model Pergunta não tem colunas específicas
    }

    public class AtualizarPerguntaDiscursivaRequest : FASTSURVEY.Dtos.Perguntas.AtualizarPerguntaRequestBase
    {
        // Idem
    }
}
