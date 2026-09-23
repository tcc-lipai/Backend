using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ItemSalvo
    {
        [Key]
        public int IdItemSalvo { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string TipoItem { get; set; } = string.Empty;
        public int ItemId { get; set; }

        public DateTime DataSalva { get; set; } = DateTime.UtcNow;
    }
}
