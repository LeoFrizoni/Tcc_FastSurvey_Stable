#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.ParticipantesSessao
{
    public sealed class ParticipanteUpdateRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string NomeParticipante { get; set; } = string.Empty;
    }
}
