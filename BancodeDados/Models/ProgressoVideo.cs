using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ProgressoVideo
    {
        [Key]
        public int IdProgresso { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int LicaoVideoId { get; set; }
        public LicaoVideo? LicaoVideo { get; set; }

        public StatusProgresso Status { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
    }
}
