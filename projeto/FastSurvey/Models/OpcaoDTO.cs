#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Models
{
    /// <summary>
    /// DTO de opção de resposta para perguntas objetivas/múltipla escolha.
    /// </summary>
    public class OpcaoDto
    {
        /// <summary>
        /// Identificador da opção (geralmente definido pelo banco). 
        /// Em cenários de criação, pode ser ignorado.
        /// </summary>
        public int Id { get; set; }

        /// <summary>Texto da opção exibido ao respondente.</summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(500, ErrorMessage = "Texto da opção pode ter no máximo 500 caracteres.")]
        [RegularExpression(@".*\S.*", ErrorMessage = "Texto da opção não pode ser vazio ou apenas espaços.")]
        public string Texto { get; set; } = string.Empty;

        /// <summary>Indica se essa opção é considerada correta (para gabarito).</summary>
        public bool Correta { get; set; } = false;

        /// <summary>Id da pergunta vinculada.</summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PerguntaId deve ser positivo.")]
        public int PerguntaId { get; set; }
    }
}
