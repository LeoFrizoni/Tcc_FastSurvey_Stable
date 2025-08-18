namespace FASTSURVEY.Models
{
    public class PerguntaMultiplaEscolhaDto
    {
        public string Titulo { get; set; } = string.Empty;

        public bool TemGabarito { get; set; } = false;

        // Se true, o responder permite marcar várias opções
        public bool PermitirMultiplaSelecao { get; set; } = true;

        // Índices corretos quando TemGabarito = true (ex.: [0,2])
        public List<int> Corretas { get; set; } = new();

        public List<OpcaoDto> Opcoes { get; set; } = new();
    }
}

