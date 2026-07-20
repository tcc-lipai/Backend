using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Profissional
    {
        [Key]
        public int IdProfissional { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string? Registro { get; set; } // CRM, CRP, etc — opcional
        public string CodigoAcesso { get; set; } = string.Empty;

        public List<Usuario> Pacientes { get; set; } = new();
    }
}