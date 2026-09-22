using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class AtividadeFala
    {
        [Key]
        public int IdAtividadeFala { get; set; }

        public string Nome { get; set; } = string.Empty;
        public NivelDificuldade NivelDificuldade { get; set; }

        public int UnidadeId { get; set; }
        public Unidade? Unidade { get; set; }

        public List<LicaoFala> Exercicios { get; set; } = new();
    }
}
