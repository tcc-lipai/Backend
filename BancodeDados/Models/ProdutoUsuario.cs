namespace Backend.Models
{
    public class ProdutoUsuario
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public DateTime DataCompra { get; set; }
        public int Qtde { get; set; }
        public decimal TotalCompra { get; set; }
    }
}
