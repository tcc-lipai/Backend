using Backend.Models;

namespace Backend.Dtos
{
    public class LicaoFalaCriacaoDto
    {
        public int UnidadeId { get; set; }
        public string FraseEsperada { get; set; } = string.Empty;
        public NivelDificuldade NivelDificuldade { get; set; }
        public decimal ValorGanho { get; set; }
    }
}