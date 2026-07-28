using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class MensagemContato
    {
        [Key]
        public int IdMensagem { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool Respondida { get; set; } = false;
    }
}