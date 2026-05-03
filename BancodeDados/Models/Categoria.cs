namespace Backend.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }

        public List<ItemDicionario> Itens { get; set; } = new();
    }
}
