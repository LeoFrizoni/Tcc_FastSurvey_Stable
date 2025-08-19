using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Models
{

    public class PerguntaMultiplaEscolhaDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PesquisaId deve ser positivo.")]
        public int PesquisaId { get; set; }

        /// <summary>Enunciado (mapeia para perguntas.texto).</summary>
        [Required]
        [StringLength(5000)]
        public string Texto { get; set; } = string.Empty;

        public bool TemGabarito { get; set; } = false;

        /// <summary>Se true, permite marcar várias opções no responder.</summary>
        public bool PermitirMultiplaSelecao { get; set; } = true;

        /// <summary>Índices das opções corretas (0-based) quando TemGabarito = true.</summary>
        public List<int> Corretas { get; set; } = new();

        /// <summary>Lista de opções (mínimo 2).</summary>
        [MinLength(2, ErrorMessage = "Inclua pelo menos 2 opções.")]
        public List<OpcaoDto> Opcoes { get; set; } = new();
    }
}
