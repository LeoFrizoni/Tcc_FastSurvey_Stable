using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Pastas
{
    public class CriarPastaRequest
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public int LoginId { get; set; }
    }

    public class RenomearPastaRequest
    {
        [Required]
        public string NovoNome { get; set; } = string.Empty;
    }

    public class PastaResponse
    {
        public int PastaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int? LoginId { get; set; }
    }
}
