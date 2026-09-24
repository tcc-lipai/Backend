using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class HistoricoOfensiva
    {
        [Key]
        public int IdHistoricoOfensiva { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        /// <summary>Data (sem hora, UTC) a que esse registro se refere.</summary>
        public DateTime Data { get; set; }

        /// <summary>"Realizada" (o usuário completou uma atividade nesse dia) ou "Congelada" (dia protegido pelo Bloqueio de Ofensiva).</summary>
        public string Status { get; set; } = "Realizada";
    }
}
