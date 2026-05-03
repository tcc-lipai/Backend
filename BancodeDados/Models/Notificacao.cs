namespace Backend.Models
{
    public class Notificacao
    {
        public int IdNotificacao { get; set; }
        public string Descricao { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
