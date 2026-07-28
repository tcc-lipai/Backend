using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Notificacao
    {
        [Key]
        public int IdNotificacao { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Lida { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow; 

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}