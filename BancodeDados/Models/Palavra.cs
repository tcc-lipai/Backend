using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Palavra
    {
        [Key]
        public int IdPalavra { get; set; }

        public string Nome { get; set; } = string.Empty; 
        public string VideoUrl { get; set; } = string.Empty; 

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}