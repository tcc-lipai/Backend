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
            var atividade = await db.Atividades.FirstOrDefaultAsync(a => a.Nome == "Leitura Labial");
            if (atividade == null)
            {
                atividade = new Atividade
                {
                    Nome = "Leitura Labial",
                    Descricao = "Treine a leitura labial e a fala com exercícios guiados.",
                };
                db.Atividades.Add(atividade);
                await db.SaveChangesAsync();
            }

            var u3 = await db.Unidades.FirstOrDefaultAsync(u => u.Nome == "Unidade 3 — Fonema A (Nível Básico)");
            if (u3 == null)
            {
                u3 = new Unidade { Nome = "Unidade 3 — Fonema A (Nível Básico)", AtividadeId = atividade.IdAtividade };
                db.Unidades.Add(u3);
                await db.SaveChangesAsync();
            }

            var atividadeFalaU3 = await ObterOuCriarAtividadeFalaAsync(db, u3.IdUnidade, "Fala A 1", NivelDificuldade.Basico);
            await SeedExerciciosFalaAsync(db, atividadeFalaU3, new (string, NivelDificuldade, decimal)[]
            {
                ("aba", NivelDificuldade.Basico, 10),
                ("acho", NivelDificuldade.Basico, 10),
                ("aço", NivelDificuldade.Basico, 10),
                ("agir", NivelDificuldade.Basico, 10),
                ("ágil", NivelDificuldade.Basico, 10),
            });

            if (!await db.LicoesVideo.AnyAsync(l => l.UnidadeId == u3.IdUnidade))
            {
                db.LicoesVideo.Add(new LicaoVideo
                {
                    UnidadeId = u3.IdUnidade,
                    Titulo = "Como pronunciar o som A",
                    VideoDescricao = "Veja como a boca se movimenta ao pronunciar palavras com o fonema A.",
                    VideoUrl = "",
                });
            }

            if (!await db.LicoesAlternativa.AnyAsync(l => l.UnidadeId == u3.IdUnidade))
            {
                db.LicoesAlternativa.Add(new LicaoAlternativa
                {
                    UnidadeId = u3.IdUnidade,
                    Pergunta = "Qual palavra a pessoa falou?",
                    A = "Aba",
                    B = "Aço",
                    C = "Agir",
                    D = "Ágil",
                    RespostaCerta = "A",
                    ValorGanho = 10,
                    VideoUrl = "",
                });
            }

            await db.SaveChangesAsync();
        }

        private static async Task<AtividadeFala> ObterOuCriarAtividadeFalaAsync(
            AppDbContext db, int unidadeId, string nome, NivelDificuldade nivel)
        {
            var atividadeFala = await db.AtividadesFala.FirstOrDefaultAsync(a => a.Nome == nome);
            if (atividadeFala == null)
            {
                atividadeFala = new AtividadeFala { Nome = nome, NivelDificuldade = nivel, UnidadeId = unidadeId };
                db.AtividadesFala.Add(atividadeFala);
                await db.SaveChangesAsync();
            }
            return atividadeFala;
        }

        private static async Task SeedExerciciosFalaAsync(
            AppDbContext db, AtividadeFala atividadeFala, (string Frase, NivelDificuldade Nivel, decimal Valor)[] frases)
        {
            for (int i = 0; i < frases.Length; i++)
            {
                var (frase, nivel, valor) = frases[i];
                var existente = await db.LicoesFala
                    .FirstOrDefaultAsync(l => l.UnidadeId == atividadeFala.UnidadeId && l.FraseEsperada == frase);

                if (existente == null)
                {
                    db.LicoesFala.Add(new LicaoFala
                    {
                        UnidadeId = atividadeFala.UnidadeId,
                        FraseEsperada = frase,
                        NivelDificuldade = nivel,
                        ValorGanho = valor,
                        AtividadeFalaId = atividadeFala.IdAtividadeFala,
                        Ordem = i,
                    });
                }
                else if (existente.AtividadeFalaId == null)
                {
                    existente.AtividadeFalaId = atividadeFala.IdAtividadeFala;
                    existente.Ordem = i;
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
