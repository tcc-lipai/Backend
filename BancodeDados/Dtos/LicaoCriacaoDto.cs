namespace Backend.Dtos
{
    public class LicaoCriacaoDto
    {
        public int UnidadeId { get; set; }

        // Define o tipo: "Alternativa", "Fala" ou "Video"
        public string TipoLicao { get; set; } = string.Empty;

        // Campos para Tipo "Alternativa"
        public string Pergunta { get; set; } = string.Empty;
        public string A { get; set; } = string.Empty;
        public string B { get; set; } = string.Empty;
        public string C { get; set; } = string.Empty;
        public string D { get; set; } = string.Empty;
        public string RespostaCerta { get; set; } = string.Empty;
        public int UsuarioId { get; set; } // Opcional no POST se puder ser nulo inicialmente

        // Campos para Tipo "Fala"
        public string FraseEsperada { get; set; } = string.Empty;

        // Campos para Tipo "Video"
        public string Titulo { get; set; } = string.Empty;
        public string VideoDescricao { get; set; } = string.Empty;
    }
}