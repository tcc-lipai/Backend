using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.Dtos
{
    public class UsuarioEdicaoAdminDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [MinLength(8, ErrorMessage = "A senha precisa ter pelo menos 8 caracteres.")]
        public string? Senha { get; set; }
        public string? Diagnostico { get; set; }

        [Range(1, 4, ErrorMessage = "Informe um nível de dificuldade válido (1 a 4).")]
        public NivelDificuldade NivelDificuldade { get; set; }

        public string? CodigoProfissional { get; set; }
    }
}