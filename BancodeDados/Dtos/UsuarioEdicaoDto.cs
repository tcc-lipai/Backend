using Backend.Models;

namespace Backend.Dtos
{
    public class UsuarioEdicaoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? Senha { get; set; }

        public string? Diagnostico { get; set; }
        public NivelDificuldade NivelDificuldade { get; set; }
    }
}