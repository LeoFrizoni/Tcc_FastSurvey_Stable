namespace FASTSURVEY.Models
{
    public class PerguntaObjetivaDto
    {
        public string Titulo { get; set; } = string.Empty;

        public bool TemGabarito { get; set; } = false;

        // Índice da opção correta quando TemGabarito = true. Se não houver gabarito, mantenha null.
        public int? CorretaIndex { get; set; }

        public List<OpcaoDto> Opcoes { get; set; } = new();
    }
}
