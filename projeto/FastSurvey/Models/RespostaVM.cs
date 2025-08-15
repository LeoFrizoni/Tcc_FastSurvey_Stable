public class RespostaVM
{
    public int PesquisaId { get; set; }

    public List<ItemRespostaVM> Respostas { get; set; }
}

public class ItemRespostaVM
{
    public string Pergunta { get; set; } // será convertido para int no controller
    public string Resposta { get; set; }
}
