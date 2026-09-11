using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public decimal SaldoAtual { get; set; }

        public string SenhaHash { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Diagnostico { get; set; }

        [Required]
        public NivelDificuldade NivelDificuldade { get; set; }
        public int? ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        public List<Notificacao> Notificacoes { get; set; } = new();
        public List<ConquistaUsuario> ConquistasUsuario { get; set; } = new();
        public List<ProdutoUsuario> ProdutoUsuarios { get; set; } = new();


        /// <summary>
        /// Estado do motor adaptativo de fala (fonema atual, nível, caixinhas de
        /// repetição espaçada...) em JSON. É produzido e consumido pelo serviço
        /// de IA; o back-end apenas persiste. Null = aluno ainda não começou.
        /// </summary>
        public string? EstadoAdaptativoIA { get; set; }

        public int DiasSeguidos { get; set; } = 0;
        public DateTime? UltimaAtividadeData { get; set; }
        public DateTime? OfensivaCongeladaAte { get; set; }
        public DateTime? MultiplicadorMoedasAte { get; set; }
        public decimal? MultiplicadorMoedasValor { get; set; }
    }
}