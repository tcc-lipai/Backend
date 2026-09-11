using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Dtos
{
    /// <summary>Resposta do GET /ia/proximo-exercicio do serviço de IA (app_llm).</summary>
    public class ProximoExercicioIaDto
    {
        [JsonPropertyName("fonema")]
        public string Fonema { get; set; } = string.Empty;

        [JsonPropertyName("nivel")]
        public int Nivel { get; set; }

        [JsonPropertyName("passo_licao")]
        public int PassoLicao { get; set; }

        [JsonPropertyName("total_passos")]
        public int TotalPassos { get; set; }

        [JsonPropertyName("modo_prova")]
        public bool ModoProva { get; set; }

        [JsonPropertyName("texto_gerado")]
        public string TextoGerado { get; set; } = string.Empty;

        [JsonPropertyName("instrucao")]
        public string Instrucao { get; set; } = string.Empty;

        [JsonPropertyName("dica")]
        public string Dica { get; set; } = string.Empty;

        [JsonPropertyName("estado_adaptativo")]
        public JsonElement EstadoAdaptativo { get; set; }
    }
}
