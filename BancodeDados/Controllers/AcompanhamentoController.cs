using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    /// <summary>
    /// Acompanhamento de um paciente (area do profissional): progresso por unidade e por grupo de fala.
    /// Sempre considera só o conteudo que o nivel atual do paciente libera (acesso cumulativo),
    /// com as mesmas regras de "concluido" que o aluno ve na trilha:
    /// - uma atividade de fala (grupo) só conta como concluida quando TODOS os exercicios dela foram acertados;
    /// - interpretacao/fala avulsa/video/acompanhante contam quando concluidos (fala e interpretacao: acertados).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Usuario,Profissional,Admin")]
    public class AcompanhamentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AcompanhamentoController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<(Usuario? Usuario, IActionResult? Bloqueio)> CarregarPacienteAutorizado(int usuarioId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Usuario" && idLogado != usuarioId) return (null, Forbid());

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);
            if (usuario == null) return (null, NotFound(new { message = "Paciente não encontrado." }));

            if (role == "Profissional" && usuario.ProfissionalId != idLogado) return (null, Forbid());

            return (usuario, null);
        }

        private record Conteudo(
            List<(int Id, int UnidadeId, int? GrupoId)> Fala,
            List<(int Id, int UnidadeId)> Alternativa,
            List<(int Id, int UnidadeId)> Video,
            List<(int Id, int UnidadeId)> Vibracao,
            List<(int Id, string Nome, NivelDificuldade Nivel, int UnidadeId)> Grupos,
            HashSet<int> FalaFeita,
            HashSet<int> AlternativaFeita,
            HashSet<int> VideoFeito,
            HashSet<int> VibracaoFeita);

        private async Task<Conteudo> CarregarConteudo(Usuario usuario, int? unidadeId)
        {
            var nivel = usuario.NivelDificuldade;
            var uid = usuario.IdUsuario;

            var fala = await _context.LicoesFala
                .Where(l => l.NivelDificuldade <= nivel && (unidadeId == null || l.UnidadeId == unidadeId))
                .Select(l => new { l.IdLicaoFala, l.UnidadeId, l.AtividadeFalaId }).ToListAsync();
            var alternativa = await _context.LicoesAlternativa
                .Where(l => l.NivelDificuldade <= nivel && (unidadeId == null || l.UnidadeId == unidadeId))
                .Select(l => new { l.IdLicaoAlternativa, l.UnidadeId }).ToListAsync();
            var video = await _context.LicoesVideo
                .Where(l => l.NivelDificuldade <= nivel && (unidadeId == null || l.UnidadeId == unidadeId))
                .Select(l => new { l.IdLicaoVideo, l.UnidadeId }).ToListAsync();
            var vibracao = await _context.LicoesVibracao
                .Where(l => unidadeId == null || l.UnidadeId == unidadeId)
                .Select(l => new { l.IdLicaoVibracao, l.UnidadeId }).ToListAsync();
            var grupos = await _context.AtividadesFala
                .Where(a => a.NivelDificuldade <= nivel && (unidadeId == null || a.UnidadeId == unidadeId))
                .Select(a => new { a.IdAtividadeFala, a.Nome, a.NivelDificuldade, a.UnidadeId }).ToListAsync();

            var falaFeita = await _context.ProgressosFala
                .Where(p => p.UsuarioId == uid && p.Status == StatusProgresso.Concluido && p.Correta)
                .Select(p => p.LicaoFalaId).Distinct().ToListAsync();
            var altFeita = await _context.ProgressosAlternativa
                .Where(p => p.UsuarioId == uid && p.Status == StatusProgresso.Concluido && p.Correta)
                .Select(p => p.LicaoAlternativaId).Distinct().ToListAsync();
            var videoFeito = await _context.ProgressosVideo
                .Where(p => p.UsuarioId == uid && p.Status == StatusProgresso.Concluido)
                .Select(p => p.LicaoVideoId).Distinct().ToListAsync();
            var vibFeita = await _context.ProgressosVibracao
                .Where(p => p.UsuarioId == uid && p.Status == StatusProgresso.Concluido)
                .Select(p => p.LicaoVibracaoId).Distinct().ToListAsync();

            return new Conteudo(
                fala.Select(l => (l.IdLicaoFala, l.UnidadeId, l.AtividadeFalaId)).ToList(),
                alternativa.Select(l => (l.IdLicaoAlternativa, l.UnidadeId)).ToList(),
                video.Select(l => (l.IdLicaoVideo, l.UnidadeId)).ToList(),
                vibracao.Select(l => (l.IdLicaoVibracao, l.UnidadeId)).ToList(),
                grupos.Select(g => (g.IdAtividadeFala, g.Nome, g.NivelDificuldade, g.UnidadeId)).ToList(),
                falaFeita.ToHashSet(), altFeita.ToHashSet(), videoFeito.ToHashSet(), vibFeita.ToHashSet());
        }

        private static (int Total, int Concluidas) ContarUnidade(Conteudo c, int unidadeId)
        {
            int total = 0, feitas = 0;

            // fala avulsa
            foreach (var l in c.Fala.Where(l => l.UnidadeId == unidadeId && l.GrupoId == null))
            {
                total++;
                if (c.FalaFeita.Contains(l.Id)) feitas++;
            }

            // grupos de fala: so contam se tiverem exercicios; concluidos quando todos foram acertados
            foreach (var g in c.Grupos.Where(g => g.UnidadeId == unidadeId))
            {
                var exercicios = c.Fala.Where(l => l.GrupoId == g.Id).Select(l => l.Id).ToList();
                if (exercicios.Count == 0) continue;
                total++;
                if (exercicios.All(c.FalaFeita.Contains)) feitas++;
            }

            foreach (var l in c.Alternativa.Where(l => l.UnidadeId == unidadeId))
            {
                total++;
                if (c.AlternativaFeita.Contains(l.Id)) feitas++;
            }
            foreach (var l in c.Video.Where(l => l.UnidadeId == unidadeId))
            {
                total++;
                if (c.VideoFeito.Contains(l.Id)) feitas++;
            }
            foreach (var l in c.Vibracao.Where(l => l.UnidadeId == unidadeId))
            {
                total++;
                if (c.VibracaoFeita.Contains(l.Id)) feitas++;
            }

            return (total, feitas);
        }

        /// <summary>Progresso de cada unidade (total de atividades, concluidas e %), no nivel atual do paciente.</summary>
        [HttpGet("paciente/{usuarioId}/unidades")]
        public async Task<IActionResult> GetUnidades(int usuarioId)
        {
            var (usuario, bloqueio) = await CarregarPacienteAutorizado(usuarioId);
            if (bloqueio != null) return bloqueio;

            var conteudo = await CarregarConteudo(usuario!, null);
            var unidades = await _context.Unidades.OrderBy(u => u.IdUnidade)
                .Select(u => new { u.IdUnidade, u.Nome }).ToListAsync();

            var lista = unidades.Select(u =>
            {
                var (total, feitas) = ContarUnidade(conteudo, u.IdUnidade);
                return new
                {
                    UnidadeId = u.IdUnidade,
                    u.Nome,
                    TotalAtividades = total,
                    AtividadesConcluidas = feitas,
                    Percentual = total == 0 ? 0 : (int)Math.Round((double)feitas / total * 100),
                    Status = total == 0 ? "SemAtividades" : feitas == total ? "Concluida" : feitas > 0 ? "EmAndamento" : "NaoIniciada"
                };
            }).ToList();

            int totalGeral = lista.Sum(x => x.TotalAtividades);
            int feitasGeral = lista.Sum(x => x.AtividadesConcluidas);

            return Ok(new
            {
                Nivel = usuario!.NivelDificuldade,
                TotalAtividades = totalGeral,
                AtividadesConcluidas = feitasGeral,
                Percentual = totalGeral == 0 ? 0 : (int)Math.Round((double)feitasGeral / totalGeral * 100),
                Unidades = lista
            });
        }

        /// <summary>Detalhe de uma unidade: total/concluidas por tipo e cada grupo de fala com o acerto do paciente.</summary>
        [HttpGet("paciente/{usuarioId}/unidades/{unidadeId}")]
        public async Task<IActionResult> GetUnidade(int usuarioId, int unidadeId)
        {
            var (usuario, bloqueio) = await CarregarPacienteAutorizado(usuarioId);
            if (bloqueio != null) return bloqueio;

            var unidade = await _context.Unidades.Where(u => u.IdUnidade == unidadeId)
                .Select(u => new { u.IdUnidade, u.Nome }).FirstOrDefaultAsync();
            if (unidade == null) return NotFound(new { message = "Unidade não encontrada." });

            var c = await CarregarConteudo(usuario!, unidadeId);
            var (total, feitas) = ContarUnidade(c, unidadeId);

            // tentativas (concluidas) de fala dos exercicios dessa unidade, pra calcular o acerto por grupo
            var idsFala = c.Fala.Select(l => l.Id).ToList();
            var tentativas = await _context.ProgressosFala
                .Where(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido && idsFala.Contains(p.LicaoFalaId))
                .Select(p => new { p.LicaoFalaId, p.Correta, p.ScoreAcustico })
                .ToListAsync();

            var grupos = c.Grupos.Select(g =>
            {
                var exercicios = c.Fala.Where(l => l.GrupoId == g.Id).Select(l => l.Id).ToList();
                var tent = tentativas.Where(t => exercicios.Contains(t.LicaoFalaId)).ToList();
                var concluidos = exercicios.Count(c.FalaFeita.Contains);
                return new
                {
                    IdAtividadeFala = g.Id,
                    g.Nome,
                    Nivel = g.Nivel,
                    TotalExercicios = exercicios.Count,
                    ExerciciosConcluidos = concluidos,
                    Percentual = exercicios.Count == 0 ? 0 : (int)Math.Round((double)concluidos / exercicios.Count * 100),
                    Tentativas = tent.Count,
                    Acerto = tent.Count == 0 ? (double?)null : Math.Round((double)tent.Count(t => t.Correta) / tent.Count * 100, 1),
                    NotaMedia = tent.Count(t => t.ScoreAcustico != null) == 0
                        ? (double?)null
                        : Math.Round(tent.Where(t => t.ScoreAcustico != null).Average(t => t.ScoreAcustico!.Value), 1)
                };
            })
            .Where(g => g.TotalExercicios > 0)
            .OrderBy(g => g.Nivel).ThenBy(g => g.IdAtividadeFala)
            .ToList();

            return Ok(new
            {
                UnidadeId = unidade.IdUnidade,
                unidade.Nome,
                Nivel = usuario!.NivelDificuldade,
                TotalAtividades = total,
                AtividadesConcluidas = feitas,
                Percentual = total == 0 ? 0 : (int)Math.Round((double)feitas / total * 100),
                Tipos = new
                {
                    Acompanhante = new { Total = c.Vibracao.Count, Concluidas = c.Vibracao.Count(l => c.VibracaoFeita.Contains(l.Id)) },
                    Video = new { Total = c.Video.Count, Concluidas = c.Video.Count(l => c.VideoFeito.Contains(l.Id)) },
                    Interpretacao = new { Total = c.Alternativa.Count, Concluidas = c.Alternativa.Count(l => c.AlternativaFeita.Contains(l.Id)) },
                    Fala = new { Total = c.Fala.Count, Concluidas = c.Fala.Count(l => c.FalaFeita.Contains(l.Id)) }
                },
                GruposFala = grupos
            });
        }
    }
}
