// FASTSURVEY/Dtos/Tipos/TipoUsuarioCatalogDto.cs
#nullable enable
namespace FASTSURVEY.Dtos.Tipos
{
    /// DTO de catálogo (listar no front). Evita vazar o nome de coluna do banco.
    public class TipoUsuarioCatalogDto
    {
        public int TipoUsuarioId { get; set; }
        public string TipoUsuario { get; set; } = string.Empty;
    }
}
