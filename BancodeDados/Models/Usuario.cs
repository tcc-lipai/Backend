namespace Backend.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public decimal SaldoAtual { get; set; }

        public string TipoUsuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Diagnostico { get; set; }

        public List<Notificacao> Notificacoes { get; set; } = new();
        public List<Conquista> Conquistas { get; set; } = new();
        public List<ProdutoUsuario> ProdutoUsuarios { get; set; } = new();
    }
}
