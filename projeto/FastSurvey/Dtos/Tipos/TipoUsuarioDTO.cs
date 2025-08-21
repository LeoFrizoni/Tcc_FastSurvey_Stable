#nullable enable
namespace FASTSURVEY.Dtos.Tipos
{
    public class TipoUsuarioDto
    {
        public int tipousuarioid { get; set; }
        public string tipousuario { get; set; } = string.Empty;
        public bool desabilitado { get; set; }
    }
}
