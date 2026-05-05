using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Produto
    {
        [Key]
        public int IdProduto { get; set; }

        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }

        public List<ProdutoUsuario> ProdutoUsuarios { get; set; } = new();
    }
}