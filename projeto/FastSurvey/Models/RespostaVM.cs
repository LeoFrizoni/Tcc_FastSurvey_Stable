// FASTSURVEY/Models/DTO/RespostasLoteVM.cs
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Models.DTO
{
    /// <summary>
    /// Representa um lote de respostas submetido por um usuário para uma pesquisa.
    /// </summary>
    public class RespostasLoteVM
    {
        /// <summary>
        /// Pesquisa à qual pertencem as respostas.
        /// </summary>
        [Required]
        public int PesquisaId { get; set; }

        /// <summary>
        /// Identificador do usuário (login) que respondeu.
        /// </summary>
        [Required]
        public int LoginId { get; set; }

        /// <summary>
        /// Lista das respostas individuais enviadas.
        /// </summary>
        [MinLength(1, ErrorMessage = "É necessário enviar ao menos uma resposta.")]
        public List<ItemRespostaLoteVM> Itens { get; set; } = new();
    }

    /// <summary>
    /// Representa uma única resposta de uma pergunta dentro de um lote.
    /// </summary>
    public class ItemRespostaLoteVM
    {
        /// <summary>
        /// Identificador da pergunta respondida.
        /// </summary>
        [Required]
        public int PerguntaId { get; set; }

        /// <summary>
        /// Tipo da pergunta: "discursiva" | "objetiva" | "multipla".
        /// </summary>
        [Required, RegularExpression("^(discursiva|objetiva|multipla)$",
            ErrorMessage = "Tipo inválido. Use 'discursiva', 'objetiva' ou 'multipla'.")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Usado quando Tipo = "discursiva".
        /// Texto livre da resposta.
        /// </summary>
        public string? Texto { get; set; }

        /// <summary>
        /// Usado quando Tipo = "objetiva" ou "multipla".
        /// IDs das opções escolhidas.
        /// </summary>
        public List<int>? Opcoes { get; set; }

        /// <summary>
        /// Opcional: anexos previamente enviados que se relacionam a essa resposta.
        /// </summary>
        public List<int>? AnexosIds { get; set; }
    }
}
