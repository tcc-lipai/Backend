using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class LicaoVibracao
    {
        [Key]
        public int IdLicaoVibracao { get; set; }

        public string Silaba { get; set; } = string.Empty;
        public string Instrucao { get; set; } = string.Empty;

        public int UnidadeId { get; set; }
        public Unidade? Unidade { get; set; }
    }
}