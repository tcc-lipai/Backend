using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class LicaoFala
    {
        [Key]
        public int IdLicaoFala { get; set; }

        public string FraseEsperada { get; set; } = string.Empty;
        public NivelDificuldade NivelDificuldade { get; set; } // 1 palavra / 2 palavras / frase
        public decimal ValorGanho { get; set; }

        public int UnidadeId { get; set; }
        public Unidade? Unidade { get; set; }
    }
}
