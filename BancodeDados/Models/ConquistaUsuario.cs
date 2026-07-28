using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ConquistaUsuario
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int ConquistaId { get; set; }
        public Conquista? Conquista { get; set; }

        public DateTime DataConquista { get; set; }
    }
}