namespace Backend.Dtos
{
    public class PalavraCriacaoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
    }
}