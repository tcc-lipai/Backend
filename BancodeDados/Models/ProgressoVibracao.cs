using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ProgressoVibracao
    {
        [Key]
        public int IdProgresso { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int LicaoVibracaoId { get; set; }
        public LicaoVibracao? LicaoVibracao { get; set; }

        public StatusProgresso Status { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
    }
}
