#nullable enable
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Pesquisas
{
    public class PesquisaInterativaRequest
    {
        [Required]
        public int PesquisaId { get; set; }
        
        public bool IsAtiva { get; set; } = false;
    }

    public class EntrarSessaoRequest
    {
        [Required]
        public string CodigoAcesso { get; set; } = string.Empty;
        
        public string? NomeParticipante { get; set; }
    }

    public class RespostaInterativaRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;
        
        [Required]
        public int ParticipanteId { get; set; }
        
        [Required]
        public int PerguntaId { get; set; }
        
        public string? TextoResposta { get; set; }
        
        public List<int> OpcoesSelecionadas { get; set; } = new();
    }

    public class AvancarPerguntaRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;
        
        public int? PerguntaId { get; set; }
        
        public bool AtivarPergunta { get; set; } = false;
    }

    public class VoltarPerguntaRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;
        
        public bool AtivarPergunta { get; set; } = false;
    }

    public class IrParaPerguntaRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;
        
        [Required]
        public int PerguntaId { get; set; }
        
        public bool AtivarPergunta { get; set; } = false;
    }

    public class AtivarPerguntaRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;
        
        public bool Ativar { get; set; } = true;
    }

    public class FinalizarSessaoRequest
    {
        [Required]
        public string SessaoId { get; set; } = string.Empty;
    }

    public class SessaoInterativaResponse
    {
        public string SessaoId { get; set; } = string.Empty;
        public int PesquisaId { get; set; }
        public string CodigoAcesso { get; set; } = string.Empty;
        public string TituloPesquisa { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? IniciadaEm { get; set; }
        public int TotalParticipantes { get; set; }
        public int? PerguntaAtualId { get; set; }
        public int? OrdemPerguntaAtual { get; set; }
        public bool ModoApresentacao { get; set; }
        public bool PerguntaAtiva { get; set; }
        public List<PerguntaInterativaResponse> Perguntas { get; set; } = new();
        public PerguntaInterativaResponse? PerguntaAtual { get; set; }
    }

    public class PerguntaInterativaResponse
    {
        public int PerguntaId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public List<OpcaoInterativaResponse> Opcoes { get; set; } = new();
    }

    public class OpcaoInterativaResponse
    {
        public int OpcaoId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }

    public class ParticipanteResponse
    {
        public int ParticipanteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string SessaoId { get; set; } = string.Empty;
        public DateTime EntrouEm { get; set; }
        public bool Ativo { get; set; }
    }

    public class RespostaInterativaResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public int RespostaId { get; set; }
    }

    public class PerguntaAtualResponse
    {
        public int PerguntaId { get; set; }
        public int Ordem { get; set; }
        public string Texto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public int TotalPerguntas { get; set; }
        public bool TemProxima { get; set; }
        public bool TemAnterior { get; set; }
        public List<OpcaoInterativaResponse> Opcoes { get; set; } = new();
    }
}
