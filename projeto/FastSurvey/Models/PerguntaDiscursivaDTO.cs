#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Models
{
    /// <summary>
    /// DTO para criar/editar pergunta discursiva.
    /// </summary>
    public class PerguntaDiscursivaDto
    {
        /// <summary>Pesquisa de destino.</summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PesquisaId deve ser positivo.")]
        public int PesquisaId { get; set; }

        /// <summary>Texto/enunciado da pergunta (mapeia para perguntas.texto).</summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(5000, ErrorMessage = "Texto pode ter no máximo 5000 caracteres.")]
        [RegularExpression(@".*\S.*", ErrorMessage = "Texto não pode ser vazio ou apenas espaços.")]
        public string Texto { get; set; } = string.Empty;

        /// <summary>Opcional: exemplo exibido em preview/template.</summary>
        [StringLength(5000, ErrorMessage = "RespostaExemplo pode ter no máximo 5000 caracteres.")]
        public string? RespostaExemplo { get; set; }
    }
}
