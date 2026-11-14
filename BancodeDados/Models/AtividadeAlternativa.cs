using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class AtividadeAlternativa
    {
        [Key]
        public int IdAtividadeAlternativa { get; set; }

        public string Nome { get; set; } = string.Empty;
        public NivelDificuldade NivelDificuldade { get; set; }

        public int UnidadeId { get; set; }
        public Unidade? Unidade { get; set; }

        public List<LicaoAlternativa> Exercicios { get; set; } = new();
    }
}
