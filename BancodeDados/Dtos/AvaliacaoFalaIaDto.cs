using System.Text.Json.Serialization;

namespace Backend.Dtos
{
    public class AvaliacaoFalaIaDto
    {
        [JsonPropertyName("texto_falado")]
        public string TextoFalado { get; set; } = string.Empty;

        [JsonPropertyName("score_geral")]
        public double ScoreGeral { get; set; }
    }
}
