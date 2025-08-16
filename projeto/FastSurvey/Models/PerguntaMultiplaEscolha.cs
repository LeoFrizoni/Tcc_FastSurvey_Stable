namespace FASTSURVEY.Models
{
    public class PerguntaMultiplaEscolha
    {
    public string? Titulo { get; set; }
        public List<OpcoesPerguntaObjetiva> Opcoes { get; set; } = new List<OpcoesPerguntaObjetiva>();
    }
}
namespace FASTSURVEY.Models
{
    public class PerguntaObjetiva
    {
        public string Titulo { get; set; }
        public List<OpcoesPerguntaObjetiva> Opcoes { get; set; } = new List<OpcoesPerguntaObjetiva>();
    }
}
