namespace Backend.Models
{
    public class Atividade
    {
        public int IdAtividade { get; set; }
        public string Nome { get; set; } = string.Empty;

        public List<Unidade> Unidades { get; set; } = new();
    }
}
