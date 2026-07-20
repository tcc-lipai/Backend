using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public decimal SaldoAtual { get; set; }

        public string SenhaHash { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Diagnostico { get; set; }

        [Required]
        public NivelDificuldade NivelDificuldade { get; set; }
        public int? ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        public List<Notificacao> Notificacoes { get; set; } = new();
        public List<Conquista> Conquistas { get; set; } = new();
        public List<ProdutoUsuario> ProdutoUsuarios { get; set; } = new();
    }
}