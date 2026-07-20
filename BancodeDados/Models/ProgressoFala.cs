using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ProgressoFala
    {
        [Key]
        public int IdProgresso { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int LicaoFalaId { get; set; }
        public LicaoFala? LicaoFala { get; set; }

        public string? TextoReconhecidoPelaIA { get; set; }
        public bool Correta { get; set; }
        public decimal PontuacaoObtida { get; set; }
        public StatusProgresso Status { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
    }
}