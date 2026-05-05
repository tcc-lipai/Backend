using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Conquista
    {
        [Key]
        public int IdConquista { get; set; }
        public string Nome { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}