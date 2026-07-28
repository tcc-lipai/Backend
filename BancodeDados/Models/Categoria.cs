using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Categoria
    {
        [Key]
        public int IdCategoria { get; set; }

        public string Nome { get; set; } = string.Empty;

        public List<Palavra> Palavras { get; set; } = new();
    }
}