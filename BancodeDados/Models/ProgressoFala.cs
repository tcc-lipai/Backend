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

        /// <summary>Nota acústica (0-100) devolvida pelo serviço de IA.</summary>
        public double? ScoreAcustico { get; set; }

        /// <summary>Feedback em linguagem natural gerado pela IA (fonoaudiólogo).</summary>
        public string? FeedbackIA { get; set; }

        /// <summary>Detalhes por fonema (JSON) devolvidos pela IA.</summary>
        public string? DetalhesFonemasJson { get; set; }

        public bool Correta { get; set; }
        public decimal PontuacaoObtida { get; set; }
        public StatusProgresso Status { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
    }
}