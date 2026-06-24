namespace Backend.Dtos
{
    public class UsuarioCadastroDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string TipoUsuario { get; set; }
        public string? Diagnostico { get; set; }
    }
}