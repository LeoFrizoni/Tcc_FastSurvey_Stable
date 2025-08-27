// FASTSURVEY/Dtos/Tipos/TipoPesquisaCatalogDto.cs
#nullable enable
namespace FASTSURVEY.Dtos.Tipos
{
    /// DTO de catálogo (listar no front). Não expõe "TipoPesquisa1" do banco.
    public class TipoPesquisaCatalogDto
    {
        public int TipoPesquisaId { get; set; }
        public string TipoPesquisa { get; set; } = string.Empty;
        public bool Desabilitado { get; set; }
    }

    /// DTO para cadastrar novo tipo de pesquisa
    public class CadastrarTipoPesquisaRequest
    {
        public string TipoPesquisa { get; set; } = string.Empty;
    }

    /// DTO para alterar tipo de pesquisa existente
    public class AlterarTipoPesquisaRequest
    {
        public string TipoPesquisa { get; set; } = string.Empty;
    }
}
