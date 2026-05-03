namespace Backend.Models
{
    public class Produto
    {
        public int IdProduto { get; set; }

        public List<ProdutoUsuario> ProdutoUsuarios { get; set; } = new();
    }
}
