using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Atividade
    {
        [Key]
        public int IdAtividade { get; set; }
        public string Nome { get; set; } = string.Empty;

        public List<Unidade> Unidades { get; set; } = new();
    }
}