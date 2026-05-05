using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ItemDicionario
    {
        [Key]
        public int IdItemDicionario { get; set; }

        public int DicionarioId { get; set; }
        public Dicionario? Dicionario { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
