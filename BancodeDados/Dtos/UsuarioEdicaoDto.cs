namespace Backend.Dtos
{
    public class UsuarioEdicaoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string? Diagnostico { get; set; }
    }
}