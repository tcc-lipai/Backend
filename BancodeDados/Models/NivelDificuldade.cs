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

    // Conversor próprio: força esse enum específico a sempre virar/ler número (1, 2, 3, 4)
    // no JSON — é isso que a IA vai receber/enviar — mesmo com a regra global de
    // "enum vira texto" configurada no Program.cs pros outros enums do projeto.
    public class NivelDificuldadeJsonConverter : JsonConverter<NivelDificuldade>
    {
        public override NivelDificuldade Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int valor = reader.GetInt32();
            return (NivelDificuldade)valor;
        }

        public override void Write(Utf8JsonWriter writer, NivelDificuldade value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }
}