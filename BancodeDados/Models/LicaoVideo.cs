using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class LicaoVideo
    {
        [Key]
        public int IdLicaoVideo { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string VideoDescricao { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public NivelDificuldade NivelDificuldade { get; set; }

        public int UnidadeId { get; set; }
        public Unidade? Unidade { get; set; }
    }
}