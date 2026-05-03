namespace Backend.Models
{
    public class Conquista
    {
        public int IdConquista { get; set; }
        public string Nome { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
