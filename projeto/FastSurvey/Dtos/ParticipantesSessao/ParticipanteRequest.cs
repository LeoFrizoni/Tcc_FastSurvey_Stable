using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.ParticipantesSessao
{
    public class ParticipanteRequest
    {
        [Required(ErrorMessage = "O ID da sessão é obrigatório")]
        public string SessaoId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do participante é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string NomeParticipante { get; set; } = string.Empty;
    }
}
