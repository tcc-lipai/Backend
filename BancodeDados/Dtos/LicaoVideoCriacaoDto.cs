namespace Backend.Dtos
{
    public class LicaoVideoCriacaoDto
    {
        public int UnidadeId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string VideoDescricao { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
    }
}