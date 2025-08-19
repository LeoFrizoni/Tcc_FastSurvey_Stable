using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Models
{
    /// <summary>
    /// Modelo para receber uploads via multipart/form-data.
    /// </summary>
    public class AnexoUploadModel
    {
        // Casa com fd.append("anexo", file) no front
        [FromForm(Name = "anexo")]
        [Required(ErrorMessage = "O arquivo (anexo) é obrigatório.")]
        public IFormFile Arquivo { get; set; } = default!;

        // Se vier preenchido, vincula o anexo à pergunta
        [FromForm]
        [Range(1, int.MaxValue, ErrorMessage = "PerguntaId deve ser positivo.")]
        public int? PerguntaId { get; set; }

        // Opcional, se quiser salvar descrição depois
        [FromForm]
        [StringLength(255, ErrorMessage = "Descrição pode ter no máximo 255 caracteres.")]
        public string? Descricao { get; set; }
    }
}
