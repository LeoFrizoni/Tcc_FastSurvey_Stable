namespace SISTEMA_FASTSURVEY.MODEL.Models
{
    public partial class respostas_opcoes
    {
        public int respostaid { get; set; }
        public int opcaoid { get; set; }

        public virtual respostas resposta { get; set; } = null!;
        public virtual opcoespergunta opcao { get; set; } = null!;
    }
}
