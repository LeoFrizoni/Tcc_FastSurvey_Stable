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

    /// DTO para cadastrar novo tipo de usuário
    public class CadastrarTipoUsuarioRequest
    {
        public string TipoUsuario { get; set; } = string.Empty;
    }

    /// DTO para alterar tipo de usuário existente
    public class AlterarTipoUsuarioRequest
    {
        public string TipoUsuario { get; set; } = string.Empty;
    }
}
