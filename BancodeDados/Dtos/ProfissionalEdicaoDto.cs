using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class ProfissionalEdicaoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [MinLength(8, ErrorMessage = "A senha precisa ter pelo menos 8 caracteres.")]
        public string? Senha { get; set; }
        public string? Registro { get; set; }
    }
}