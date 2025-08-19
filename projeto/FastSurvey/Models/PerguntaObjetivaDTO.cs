using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Models
{
    public class PerguntaObjetivaDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PesquisaId deve ser positivo.")]
        public int PesquisaId { get; set; }

        /// <summary>Enunciado da pergunta (mapeia para perguntas.texto).</summary>
        [Required]
        [StringLength(5000)]
        public string Texto { get; set; } = string.Empty;

        public bool TemGabarito { get; set; } = false;

        /// <summary>Índice 0-based da opção correta quando TemGabarito = true.</summary>
        public int? CorretaIndex { get; set; }

        /// <summary>Lista de opções (mínimo 2).</summary>
        [MinLength(2, ErrorMessage = "Inclua pelo menos 2 opções.")]
        public List<OpcaoDto> Opcoes { get; set; } = new();
    }
}
