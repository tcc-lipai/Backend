using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    /// <summary>Dados que o próprio profissional pode alterar no seu perfil.</summary>
    public class ProfissionalPerfilDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MinLength(8, ErrorMessage = "A senha precisa ter pelo menos 8 caracteres.")]
        public string? Senha { get; set; }

        public string? Registro { get; set; }
    }
}
