namespace Backend.Dtos
{
    public class UnidadeResumoDto
    {
        public int IdUnidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int AtividadeId { get; set; }
        public int TotalAtividades { get; set; }
        public int TotalLicoes { get; set; }
    }
}
