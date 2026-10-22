namespace Backend.Models
{
    /// <summary>
    /// Vinculo entre um aluno e um profissional. Um aluno pode ter ate
    /// <see cref="Maximo"/> profissionais vinculados ao mesmo tempo.
    /// </summary>
    public class VinculoProfissional
    {
        public const int Maximo = 3;

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        public DateTime DataVinculo { get; set; } = DateTime.UtcNow;
    }
}
