namespace Backend.Models
{
    public class Dicionario
    {
        public int IdDicionario { get; set; }

        public List<ItemDicionario> Itens { get; set; } = new();
    }
}
