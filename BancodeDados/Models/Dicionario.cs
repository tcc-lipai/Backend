using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Dicionario
    {
        [Key]
        public int IdDicionario { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public List<ItemDicionario> Itens { get; set; } = new();
    }
}