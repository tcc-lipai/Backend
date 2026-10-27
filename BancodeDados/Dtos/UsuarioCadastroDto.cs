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

        [Required, MinLength(8, ErrorMessage = "A senha precisa ter pelo menos 8 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Informe um nível de dificuldade válido (1 a 4).")]
        public NivelDificuldade NivelDificuldade { get; set; }

        public string? Diagnostico { get; set; }


        public string? CodigoProfissional { get; set; }
    }
}