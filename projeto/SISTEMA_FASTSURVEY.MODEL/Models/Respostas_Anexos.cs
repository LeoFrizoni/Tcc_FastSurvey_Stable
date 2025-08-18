namespace SISTEMA_FASTSURVEY.MODEL.Models
{
    public partial class respostas_anexos
    {
        public int respostaid { get; set; }
        public int anexoid { get; set; }

        public virtual respostas resposta { get; set; } = null!;
        public virtual anexos anexo { get; set; } = null!;
    }
}
