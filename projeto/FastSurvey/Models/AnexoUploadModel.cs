using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Models
{
    /// <summary>
    /// Modelo para receber uploads via multipart/form-data.
    /// </summary>
    public class AnexoUploadModel
    {
        // casa com fd.append("anexo", file) no front
        [FromForm(Name = "anexo")]
        public IFormFile Arquivo { get; set; } = default!;

        // se vier preenchido, vincula o anexo à pergunta
        public int? PerguntaId { get; set; }

        // opcional, se quiser salvar descrição depois
        public string? Descricao { get; set; }
    }
}
