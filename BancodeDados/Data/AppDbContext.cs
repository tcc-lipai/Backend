using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Profissional> Profissionais { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<Unidade> Unidades { get; set; }
        public DbSet<LicaoFala> LicoesFala { get; set; }
        public DbSet<LicaoVideo> LicoesVideo { get; set; }
        public DbSet<LicaoAlternativa> LicoesAlternativa { get; set; }
        public DbSet<Conquista> Conquistas { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Palavra> Palavras { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<ProdutoUsuario> ProdutoUsuarios { get; set; }
        public DbSet<LicaoVibracao> LicoesVibracao { get; set; }
        public DbSet<ProgressoAlternativa> ProgressosAlternativa { get; set; }
        public DbSet<ProgressoFala> ProgressosFala { get; set; }
        public DbSet<ConquistaUsuario> ConquistasUsuario { get; set; }
        public DbSet<MensagemContato> MensagensContato { get; set; }
        public DbSet<AtividadeSalva> AtividadesSalvas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // E-mail único (Usuario)
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // E-mail e Código de Acesso únicos (Profissional)
            modelBuilder.Entity<Profissional>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Profissional>()
                .HasIndex(p => p.CodigoAcesso)
                .IsUnique();

            // Vínculo opcional Usuario -> Profissional
            // Se o profissional for removido, o usuário não é apagado, só desvincula.
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Profissional)
                .WithMany(p => p.Pacientes)
                .HasForeignKey(u => u.ProfissionalId)
                .OnDelete(DeleteBehavior.SetNull);

            // Tabela associativa Produto <-> Usuario
            modelBuilder.Entity<ProdutoUsuario>()
                .HasOne(pu => pu.Usuario)
                .WithMany(u => u.ProdutoUsuarios)
                .HasForeignKey(pu => pu.UsuarioId);

            modelBuilder.Entity<ProdutoUsuario>()
                .HasOne(pu => pu.Produto)
                .WithMany(p => p.ProdutoUsuarios)
                .HasForeignKey(pu => pu.ProdutoId);

            // Precisão decimal
            modelBuilder.Entity<Usuario>()
                .Property(u => u.SaldoAtual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LicaoFala>()
                .Property(l => l.ValorGanho)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProdutoUsuario>()
                .Property(p => p.TotalCompra)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LicaoAlternativa>()
                .Property(l => l.ValorGanho)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProgressoAlternativa>()
                .Property(p => p.PontuacaoObtida)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProgressoFala>()
                .Property(p => p.PontuacaoObtida)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AtividadeSalva>()
                .HasIndex(a => new { a.UsuarioId, a.AtividadeId })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}