using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class AtividadeSalva
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int AtividadeId { get; set; }
        public Atividade? Atividade { get; set; }

        public DateTime DataSalva { get; set; } = DateTime.UtcNow;
    }
}