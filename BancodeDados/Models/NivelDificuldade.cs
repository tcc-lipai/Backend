using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    [JsonConverter(typeof(NivelDificuldadeJsonConverter))]
    public enum NivelDificuldade
    {
        Iniciante = 1,
        Basico = 2,
        Intermediario = 3,
        Avancado = 4
    }

    public class NivelDificuldadeJsonConverter : JsonConverter<NivelDificuldade>
    {
        public override NivelDificuldade Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var nome = reader.GetString() ?? string.Empty;
                if (Enum.TryParse<NivelDificuldade>(nome, ignoreCase: true, out var porNome))
                    return porNome;
                throw new JsonException($"Nível de dificuldade inválido: '{nome}'.");
            }
            return (NivelDificuldade)reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, NivelDificuldade value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }
}