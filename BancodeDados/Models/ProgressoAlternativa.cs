using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ProgressoAlternativa
    {
        [Key]
        public int IdProgresso { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int LicaoAlternativaId { get; set; }
        public LicaoAlternativa? LicaoAlternativa { get; set; }

        public string? RespostaDada { get; set; }
        public bool Correta { get; set; }
        public decimal PontuacaoObtida { get; set; }
        public StatusProgresso Status { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
    }
}