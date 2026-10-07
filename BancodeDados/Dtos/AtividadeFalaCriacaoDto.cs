using Backend.Models;

namespace Backend.Dtos
{
    public class AtividadeFalaCriacaoDto
    {
        public string Nome { get; set; } = string.Empty;
        public NivelDificuldade NivelDificuldade { get; set; }
        public int UnidadeId { get; set; }
    }
}
