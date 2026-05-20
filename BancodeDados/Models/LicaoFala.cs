using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class LicaoFala
    {
        [Key]
        public int IdLicaoFala { get; set; }

        public string FraseEsperada { get; set; } = string.Empty;
        public string? FraseFalada { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }

        public decimal ValorGanho { get; set; }

        public int UnidadeId { get; set; }
        public Unidade? Unidade { get; set; }
    }
}
