using System.ComponentModel.DataAnnotations; 
using Backend.Models;                      

public class LicaoAlternativa
{
    [Key]
    public int IdLicaoAlternativa { get; set; }

    public string Pergunta { get; set; } = string.Empty;
    public string A { get; set; } = string.Empty;
    public string B { get; set; } = string.Empty;
    public string C { get; set; } = string.Empty;
    public string D { get; set; } = string.Empty;
    public string RespostaCerta { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }
    public DateTime? DataConclusao { get; set; }
    public decimal ValorGanho { get; set; }

    public int UnidadeId { get; set; }
    public Unidade? Unidade { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
