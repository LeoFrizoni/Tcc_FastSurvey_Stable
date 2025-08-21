using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Anexos
{
    /// Modelo para receber uploads via multipart/form-data.
    public class AnexoUploadRequest : IValidatableObject
    {
        // Casa com fd.append("anexo", file) no front
        [FromForm(Name = "anexo")]
        [Required(ErrorMessage = "O arquivo (anexo) é obrigatório.")]
        public IFormFile Arquivo { get; set; } = default!;

        // Vincula o anexo à pesquisa (opcional)
        [FromForm]
        [Range(1, int.MaxValue, ErrorMessage = "PesquisaId deve ser positivo.")]
        public int? PesquisaId { get; set; }

        // Vincula o anexo à pergunta (opcional)
        [FromForm]
        [Range(1, int.MaxValue, ErrorMessage = "PerguntaId deve ser positivo.")]
        public int? PerguntaId { get; set; }

        // Opcional
        [FromForm]
        [StringLength(255, ErrorMessage = "Descrição pode ter no máximo 255 caracteres.")]
        public string? Descricao { get; set; }

        // Regras adicionais
        private const long MaxTamanhoBytes = 15 * 1024 * 1024; // 15 MB
        private static readonly string[] TiposPermitidos =
            { "image/png", "image/jpeg", "application/pdf" };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Pelo menos um alvo (Pesquisa ou Pergunta)
            if (PesquisaId is null && PerguntaId is null)
            {
                yield return new ValidationResult(
                    "Informe PesquisaId ou PerguntaId.",
                    new[] { nameof(PesquisaId), nameof(PerguntaId) });
            }

            if (Arquivo is null || Arquivo.Length == 0)
            {
                yield return new ValidationResult(
                    "O arquivo enviado está vazio.", new[] { nameof(Arquivo) });
            }
            else
            {
                if (Arquivo.Length > MaxTamanhoBytes)
                    yield return new ValidationResult(
                        $"Tamanho máximo permitido é {MaxTamanhoBytes / (1024 * 1024)} MB.",
                        new[] { nameof(Arquivo) });

                if (Array.IndexOf(TiposPermitidos, Arquivo.ContentType) < 0)
                    yield return new ValidationResult(
                        "Tipo de arquivo não permitido (use PNG, JPEG ou PDF).",
                        new[] { nameof(Arquivo) });
            }
        }
    }
}
