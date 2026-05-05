using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<Unidade> Unidades { get; set; }
        public DbSet<LicaoFala> LicoesFala { get; set; }
        public DbSet<LicaoVideo> LicoesVideo { get; set; }
        public DbSet<LicaoAlternativa> LicoesAlternativa { get; set; }
        public DbSet<Conquista> Conquistas { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Dicionario> Dicionarios { get; set; }
        public DbSet<ItemDicionario> ItensDicionario { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<ProdutoUsuario> ProdutoUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. SEGURANÇA: E-mail Único
            // Impede que o banco aceite dois cadastros com o mesmo e-mail.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 2. SEGURANÇA: Configuração da Tabela Associativa (Compra de Produtos)
            // Garante que a relação N:N entre Produto e Usuario seja bem definida.
            modelBuilder.Entity<ProdutoUsuario>()
                .HasOne(pu => pu.Usuario)
                .WithMany(u => u.ProdutoUsuarios)
                .HasForeignKey(pu => pu.UsuarioId);

            modelBuilder.Entity<ProdutoUsuario>()
                .HasOne(pu => pu.Produto)
                .WithMany(p => p.ProdutoUsuarios)
                .HasForeignKey(pu => pu.ProdutoId);

            // 3. SEGURANÇA: Precisão Decimal
            // Para evitar erros de arredondamento em valores financeiros (Saldo e Ganhos).
            modelBuilder.Entity<Usuario>()
                .Property(u => u.SaldoAtual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LicaoFala>()
                .Property(l => l.ValorGanho)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProdutoUsuario>()
                .Property(p => p.TotalCompra)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}