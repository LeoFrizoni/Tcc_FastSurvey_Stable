namespace FASTSURVEY.Models.DTO
{
    public class RespostaVM
    {
        public int PesquisaId { get; set; }

        // Lista de respostas que o usuário deu
        public List<ItemRespostaVM> Respostas { get; set; } = new List<ItemRespostaVM>();
    }

    public class ItemRespostaVM
    {
        // Id da pergunta (front pode mandar como string e converteremos para int no controller)
        public string Pergunta { get; set; }

        // Resposta do usuário:
        // - Discursiva: texto livre
        // - Objetiva: id da opção escolhida
        // - Múltipla: ids separados por vírgula ou array no front
        public string Resposta { get; set; }
    }
}
