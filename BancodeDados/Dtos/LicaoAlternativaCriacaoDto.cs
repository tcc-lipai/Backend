namespace Backend.Dtos
{
    public class LicaoAlternativaCriacaoDto
    {
        public int UnidadeId { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public string Pergunta { get; set; } = string.Empty;
        public string A { get; set; } = string.Empty;
        public string B { get; set; } = string.Empty;
        public string C { get; set; } = string.Empty;
        public string D { get; set; } = string.Empty;
        public string RespostaCerta { get; set; } = string.Empty;
        public decimal ValorGanho { get; set; }
    }
}