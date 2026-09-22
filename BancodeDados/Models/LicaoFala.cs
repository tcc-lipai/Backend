using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class LicaoFala
    {
        [Key]
        public int IdLicaoFala { get; set; }

        public string FraseEsperada { get; set; } = string.Empty;
        public NivelDificuldade NivelDificuldade { get; set; }
        public decimal ValorGanho { get; set; }

        public int UnidadeId { get; set; }
        public Unidade? Unidade { get; set; }

        public int Ordem { get; set; }
        public int? AtividadeFalaId { get; set; }
        public AtividadeFala? AtividadeFala { get; set; }
    }
}
