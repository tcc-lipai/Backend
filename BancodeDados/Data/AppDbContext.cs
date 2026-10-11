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
        public DbSet<ProgressoVideo> ProgressosVideo { get; set; }
        public DbSet<ProgressoVibracao> ProgressosVibracao { get; set; }
        public DbSet<AtividadeFala> AtividadesFala { get; set; }
        public DbSet<ConquistaUsuario> ConquistasUsuario { get; set; }
        public DbSet<MensagemContato> MensagensContato { get; set; }
        public DbSet<ItemSalvo> ItensSalvos { get; set; }
        public DbSet<HistoricoOfensiva> HistoricosOfensiva { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Profissional>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Profissional>()
                .HasIndex(p => p.CodigoAcesso)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Profissional)
                .WithMany(p => p.Pacientes)
                .HasForeignKey(u => u.ProfissionalId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProdutoUsuario>()
                .HasOne(pu => pu.Usuario)
                .WithMany(u => u.ProdutoUsuarios)
                .HasForeignKey(pu => pu.UsuarioId);

            modelBuilder.Entity<ProdutoUsuario>()
                .HasOne(pu => pu.Produto)
                .WithMany(p => p.ProdutoUsuarios)
                .HasForeignKey(pu => pu.ProdutoId);

            modelBuilder.Entity<ConquistaUsuario>()
                .HasOne(cu => cu.Usuario)
                .WithMany(u => u.ConquistasUsuario)
                .HasForeignKey(cu => cu.UsuarioId);

            modelBuilder.Entity<ConquistaUsuario>()
                .HasOne(cu => cu.Conquista)
                .WithMany()
                .HasForeignKey(cu => cu.ConquistaId);

            modelBuilder.Entity<ConquistaUsuario>()
                .HasIndex(cu => new { cu.UsuarioId, cu.ConquistaId })
                .IsUnique();

            modelBuilder.Entity<ProgressoAlternativa>()
                .HasOne(p => p.LicaoAlternativa)
                .WithMany()
                .HasForeignKey(p => p.LicaoAlternativaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProgressoFala>()
                .HasOne(p => p.LicaoFala)
                .WithMany()
                .HasForeignKey(p => p.LicaoFalaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProgressoVideo>()
                .HasOne(p => p.LicaoVideo)
                .WithMany()
                .HasForeignKey(p => p.LicaoVideoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProgressoVibracao>()
                .HasOne(p => p.LicaoVibracao)
                .WithMany()
                .HasForeignKey(p => p.LicaoVibracaoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AtividadeFala>()
                .HasOne(af => af.Unidade)
                .WithMany(u => u.AtividadesFala)
                .HasForeignKey(af => af.UnidadeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LicaoFala>()
                .HasOne(l => l.AtividadeFala)
                .WithMany(af => af.Exercicios)
                .HasForeignKey(l => l.AtividadeFalaId)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<ItemSalvo>()
                .HasIndex(i => new { i.UsuarioId, i.TipoItem, i.ItemId })
                .IsUnique();

            modelBuilder.Entity<HistoricoOfensiva>()
                .HasIndex(h => new { h.UsuarioId, h.Data })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}