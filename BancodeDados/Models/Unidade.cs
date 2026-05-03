namespace Backend.Models
{
    public class Unidade
    {
        public int IdUnidade { get; set; }
        public string Nome { get; set; } = string.Empty;

        public int AtividadeId { get; set; }
        public Atividade? Atividade { get; set; }

        public List<LicaoFala> LicoesFala { get; set; } = new();
        public List<LicaoVideo> LicoesVideo { get; set; } = new();
        public List<LicaoAlternativa> LicoesAlternativa { get; set; } = new();
    }
}
