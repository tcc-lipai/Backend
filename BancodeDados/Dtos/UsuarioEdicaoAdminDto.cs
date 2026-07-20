using Backend.Models;

namespace Backend.Dtos
{
    public class UsuarioEdicaoAdminDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Senha { get; set; } // opcional
        public string? Diagnostico { get; set; }
        public NivelDificuldade NivelDificuldade { get; set; }

        public string? CodigoProfissional { get; set; }
    }
}