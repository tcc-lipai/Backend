using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class ProfissionalCadastroDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8, ErrorMessage = "A senha precisa ter pelo menos 8 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        public string? Registro { get; set; }
    }
}