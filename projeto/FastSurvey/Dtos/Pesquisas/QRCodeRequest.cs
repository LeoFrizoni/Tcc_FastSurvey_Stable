#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Pesquisas
{
    public class QRCodeRequest
    {
        [Required(ErrorMessage = "Informe o id da pesquisa.")]
        [Range(1, int.MaxValue, ErrorMessage = "PesquisaId inválido.")]
        public int PesquisaId { get; set; }

        /// <summary>
        /// Se true, força gerar um novo QR/link e persistir na pesquisa.
        /// </summary>
        public bool GerarNovo { get; set; } = false;

        /// <summary>
        /// Opcional: define/atualiza a expiração (DataFechamento) da pesquisa.
        /// </summary>
        public DateTime? Expiracao { get; set; }
    }
}
