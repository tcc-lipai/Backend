using Backend.Models;

namespace Backend.Dtos
{
    public class ProdutoCriacaoDto
    {
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public TipoProduto Tipo { get; set; } = TipoProduto.Cosmetico;
        public int? DuracaoDias { get; set; }
        public decimal? ValorMultiplicador { get; set; }
    }
}