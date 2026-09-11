using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    /// <summary>
    /// Dados de exemplo para desenvolvimento. Cada bloco só insere se a tabela
    /// correspondente estiver vazia — pode rodar quantas vezes quiser.
    /// Chamado no Program.cs quando o ambiente é Development.
    /// </summary>
    public static class SeedData
    {
        public static async Task PopularAsync(AppDbContext db)
        {
            await SeedConquistasAsync(db);
            await SeedDicionarioAsync(db);
            await SeedProdutosAsync(db);
            await SeedConteudoAsync(db);
        }

        private static async Task SeedConquistasAsync(AppDbContext db)
        {
            if (await db.Conquistas.AnyAsync(c => c.Nome == "Primeiros Passos")) return;

            // limpa conquistas de teste/placeholder que ninguém ganhou ainda
            var lixo = await db.Conquistas
                .Where(c => !db.ConquistasUsuario.Any(cu => cu.ConquistaId == c.IdConquista))
                .ToListAsync();
            db.Conquistas.RemoveRange(lixo);

            db.Conquistas.AddRange(
                new Conquista { Nome = "Primeiros Passos", Descricao = "Concluiu a sua primeira atividade." },
                new Conquista { Nome = "Semana de Ouro", Descricao = "Praticou 7 dias seguidos." },
                new Conquista { Nome = "Voz Firme", Descricao = "Acertou 10 atividades de fala." },
                new Conquista { Nome = "Colecionador", Descricao = "Comprou o primeiro item na loja." },
                new Conquista { Nome = "Dedicação Total", Descricao = "Praticou 30 dias seguidos." }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedDicionarioAsync(AppDbContext db)
        {
            if (await db.Categorias.AnyAsync(c => c.Nome == "Saudações")) return;

            // o dicionário é um catálogo global (sem progresso do usuário atrelado),
            // então dá pra limpar o que tinha antes com segurança
            db.Palavras.RemoveRange(db.Palavras);
            db.Categorias.RemoveRange(db.Categorias);
            await db.SaveChangesAsync();

            db.Categorias.AddRange(
                new Categoria
                {
                    Nome = "Saudações",
                    Palavras =
                    {
                        new Palavra { Nome = "Olá" },
                        new Palavra { Nome = "Bom dia" },
                        new Palavra { Nome = "Boa noite" },
                        new Palavra { Nome = "Tudo bem" },
                    },
                },
                new Categoria
                {
                    Nome = "Comida",
                    Palavras =
                    {
                        new Palavra { Nome = "Água" },
                        new Palavra { Nome = "Pão" },
                        new Palavra { Nome = "Arroz" },
                        new Palavra { Nome = "Maçã" },
                    },
                },
                new Categoria
                {
                    Nome = "Escola",
                    Palavras =
                    {
                        new Palavra { Nome = "Livro" },
                        new Palavra { Nome = "Caneta" },
                        new Palavra { Nome = "Professor" },
                    },
                },
                new Categoria
                {
                    Nome = "Família",
                    Palavras =
                    {
                        new Palavra { Nome = "Mãe" },
                        new Palavra { Nome = "Pai" },
                        new Palavra { Nome = "Irmão" },
                    },
                }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedProdutosAsync(AppDbContext db)
        {
            if (await db.Produtos.AnyAsync(p => p.Nome == "Bloqueio de Ofensiva")) return;

            // limpa produtos de teste/placeholder que ninguém comprou ainda
            var lixo = await db.Produtos
                .Where(p => !db.ProdutoUsuarios.Any(pu => pu.ProdutoId == p.IdProduto))
                .ToListAsync();
            db.Produtos.RemoveRange(lixo);

            db.Produtos.AddRange(
                new Produto
                {
                    Nome = "Bloqueio de Ofensiva",
                    Preco = 500,
                    Tipo = TipoProduto.BloqueioOfensiva,
                    DuracaoDias = 1,
                },
                new Produto
                {
                    Nome = "Multiplicador de Moedas x2",
                    Preco = 800,
                    Tipo = TipoProduto.MultiplicadorMoedas,
                    DuracaoDias = 1,
                    ValorMultiplicador = 2,
                },
                new Produto { Nome = "Baú Misterioso", Preco = 300, Tipo = TipoProduto.Cosmetico }
            );
            await db.SaveChangesAsync();
        }

        private static async Task SeedConteudoAsync(AppDbContext db)
        {
            if (await db.Atividades.AnyAsync(a => a.Nome == "Leitura Labial")) return;

            var atividade = new Atividade
            {
                Nome = "Leitura Labial",
                Descricao = "Treine a leitura labial e a fala com exercícios guiados.",
            };
            db.Atividades.Add(atividade);
            await db.SaveChangesAsync();

            var u1 = new Unidade { Nome = "Unidade 1 — Saudações", AtividadeId = atividade.IdAtividade };
            var u2 = new Unidade { Nome = "Unidade 2 — Dia a dia", AtividadeId = atividade.IdAtividade };
            db.Unidades.AddRange(u1, u2);
            await db.SaveChangesAsync();

            db.LicoesVideo.AddRange(
                new LicaoVideo
                {
                    UnidadeId = u1.IdUnidade,
                    Titulo = "Como ler os lábios",
                    VideoDescricao = "Introdução aos movimentos da boca na leitura labial.",
                    VideoUrl = "",
                },
                new LicaoVideo
                {
                    UnidadeId = u2.IdUnidade,
                    Titulo = "Sons parecidos",
                    VideoDescricao = "Diferenciando P, B e M ao ler os lábios.",
                    VideoUrl = "",
                }
            );

            db.LicoesAlternativa.AddRange(
                new LicaoAlternativa
                {
                    UnidadeId = u1.IdUnidade,
                    Pergunta = "Qual palavra a pessoa falou?",
                    A = "Bola",
                    B = "Bala",
                    C = "Mala",
                    D = "Fala",
                    RespostaCerta = "A",
                    ValorGanho = 10,
                    VideoUrl = "",
                },
                new LicaoAlternativa
                {
                    UnidadeId = u2.IdUnidade,
                    Pergunta = "O que ela disse?",
                    A = "Casa",
                    B = "Caça",
                    C = "Taça",
                    D = "Massa",
                    RespostaCerta = "A",
                    ValorGanho = 10,
                    VideoUrl = "",
                }
            );

            db.LicoesFala.AddRange(
                new LicaoFala
                {
                    UnidadeId = u1.IdUnidade,
                    FraseEsperada = "bom dia",
                    NivelDificuldade = NivelDificuldade.Basico,
                    ValorGanho = 10,
                },
                new LicaoFala
                {
                    UnidadeId = u1.IdUnidade,
                    FraseEsperada = "boa noite",
                    NivelDificuldade = NivelDificuldade.Basico,
                    ValorGanho = 10,
                },
                new LicaoFala
                {
                    UnidadeId = u2.IdUnidade,
                    FraseEsperada = "tudo bem",
                    NivelDificuldade = NivelDificuldade.Intermediario,
                    ValorGanho = 15,
                }
            );

            db.LicoesVibracao.AddRange(
                new LicaoVibracao
                {
                    UnidadeId = u1.IdUnidade,
                    Silaba = "PA",
                    Instrucao = "Sinta a vibração ao juntar os lábios e soltar o ar.",
                },
                new LicaoVibracao
                {
                    UnidadeId = u1.IdUnidade,
                    Silaba = "MA",
                    Instrucao = "Sinta a vibração no nariz ao fechar os lábios.",
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
