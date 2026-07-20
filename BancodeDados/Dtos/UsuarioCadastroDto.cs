using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.Dtos
{
    public class UsuarioCadastroDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Senha { get; set; } = string.Empty;

        [Required]
        public NivelDificuldade NivelDificuldade { get; set; }

        public string? Diagnostico { get; set; }


        public string? CodigoProfissional { get; set; }
    }
}