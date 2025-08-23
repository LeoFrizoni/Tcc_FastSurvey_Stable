#nullable enable
namespace FASTSURVEY.Dtos.Tipos
{
    public class TipoUsuarioDto
    {
        public int TipoUsuarioId { get; set; }
        public string TipoUsuario { get; set; } = string.Empty;
        public bool Desabilitado { get; set; }
    }
}
