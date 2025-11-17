using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FastSurvey.Dtos.Pesquisas
{
    /// <summary>
    /// Representa um bloco de pergunta no TemplateJson
    /// </summary>
    public class BlocoTemplateDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("texto")]
        public string Texto { get; set; } = string.Empty;

        [JsonPropertyName("obrigatorio")]
        public bool Obrigatorio { get; set; }

        [JsonPropertyName("ordem")]
        public int? Ordem { get; set; }

        [JsonPropertyName("opcoes")]
        public List<OpcaoTemplateDto>? Opcoes { get; set; }

        [JsonPropertyName("temGabarito")]
        public bool? TemGabarito { get; set; }

        [JsonPropertyName("permitirMultiplaSelecao")]
        public bool? PermitirMultiplaSelecao { get; set; }

        [JsonPropertyName("corretaIndex")]
        public int? CorretaIndex { get; set; }

        [JsonPropertyName("pontuacaoTotal")]
        public int? PontuacaoTotal { get; set; }

        [JsonPropertyName("tempoLimite")]
        public int? TempoLimite { get; set; }

        [JsonPropertyName("mostrarExplicacao")]
        public bool? MostrarExplicacao { get; set; }

        [JsonPropertyName("perguntaid")]
        public string? PerguntaId { get; set; }

        [JsonPropertyName("estilo")]
        public object? Estilo { get; set; }

        [JsonPropertyName("imagens")]
        public List<object>? Imagens { get; set; }
    }

    /// <summary>
    /// Representa uma opção de resposta no TemplateJson
    /// </summary>
    public class OpcaoTemplateDto
    {
        [JsonPropertyName("opcaoid")]
        public int? OpcaoId { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("texto")]
        public string Texto { get; set; } = string.Empty;

        [JsonPropertyName("correta")]
        public bool? Correta { get; set; }

        [JsonPropertyName("ordem")]
        public int? Ordem { get; set; }

        [JsonPropertyName("pontuacao")]
        public int? Pontuacao { get; set; }

        [JsonPropertyName("explicacao")]
        public string? Explicacao { get; set; }
    }
}
