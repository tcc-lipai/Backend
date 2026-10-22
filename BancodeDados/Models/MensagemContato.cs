using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class MensagemContato
    {
        [Key]
        public int IdMensagem { get; set; }

        // a mensagem vem de um aluno (UsuarioId) OU de um profissional (ProfissionalId)
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int? ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool Respondida { get; set; } = false;
    }
}