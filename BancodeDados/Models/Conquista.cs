using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Conquista
    {
        [Key]
        public int IdConquista { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? IconeUrl { get; set; }
    }
}