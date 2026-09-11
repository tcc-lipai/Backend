using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Dtos
{
    /// <summary>Resposta do POST /ia/avaliar-fala do serviço de IA (app_llm).</summary>
    public class AvaliacaoFalaIaDto
    {
        [JsonPropertyName("sucesso")]
        public bool Sucesso { get; set; } = true;

        [JsonPropertyName("texto_falado")]
        public string TextoFalado { get; set; } = string.Empty;

        [JsonPropertyName("score_geral")]
        public double ScoreGeral { get; set; }

        [JsonPropertyName("detalhes_fonemas")]
        public List<DetalheFonemaDto> DetalhesFonemas { get; set; } = new();

        [JsonPropertyName("feedback_clinico")]
        public string FeedbackClinico { get; set; } = string.Empty;

        [JsonPropertyName("feedback_origem")]
        public string? FeedbackOrigem { get; set; }

        [JsonPropertyName("decisao_adaptativa")]
        public JsonElement DecisaoAdaptativa { get; set; }

        [JsonPropertyName("estado_adaptativo")]
        public JsonElement EstadoAdaptativo { get; set; }
    }

    public class DetalheFonemaDto
    {
        [JsonPropertyName("caractere")]
        public string Caractere { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
