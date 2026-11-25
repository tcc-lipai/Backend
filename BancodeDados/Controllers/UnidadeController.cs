using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnidadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnidadesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Unidade>>> GetUnidades()
        {
            return await _context.Unidades
                .Include(u => u.LicoesFala)
                .Include(u => u.LicoesVideo)
                .Include(u => u.LicoesAlternativa)
                .Include(u => u.LicoesVibracao)
                .Include(u => u.AtividadesFala).ThenInclude(af => af.Exercicios.OrderBy(e => e.Ordem))
                .Include(u => u.AtividadesAlternativa).ThenInclude(aa => aa.Exercicios.OrderBy(e => e.IdLicaoAlternativa))
                .AsSplitQuery().ToListAsync();
        }

        /// <summary>
        /// Versão leve de GET /api/Unidades: só nome e contagem de atividades por
        /// unidade, sem trazer o conteúdo de cada exercício. Pensada para telas que
        /// só precisam listar/contar unidades (Dashboard, listagens), evitando
        /// serializar o grafo inteiro (milhares de lições) a cada carregamento.
        /// </summary>
        [HttpGet("resumo")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UnidadeResumoDto>>> GetUnidadesResumo()
        {
            // pro aluno, conta so o conteudo que o nivel dele ja libera (acesso cumulativo);
            // Admin/Profissional veem a contagem total.
            int nivelMaximo = (int)NivelDificuldade.Avancado;
            if (User.FindFirstValue(ClaimTypes.Role) == "Usuario")
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var nivelUsuario = await _context.Usuarios
                    .Where(x => x.IdUsuario == usuarioId)
                    .Select(x => (int?)x.NivelDificuldade)
                    .FirstOrDefaultAsync();
                if (nivelUsuario != null) nivelMaximo = nivelUsuario.Value;
            }

            return await _context.Unidades
                .Select(u => new UnidadeResumoDto
                {
                    IdUnidade = u.IdUnidade,
                    Nome = u.Nome,
                    AtividadeId = u.AtividadeId,
                    TotalAtividades =
                        u.LicoesFala.Count(l => l.AtividadeFalaId == null && (int)l.NivelDificuldade <= nivelMaximo)
                        + u.AtividadesFala.Count(a => (int)a.NivelDificuldade <= nivelMaximo)
                        + u.LicoesVideo.Count(l => (int)l.NivelDificuldade <= nivelMaximo)
                        + u.LicoesAlternativa.Count(l => l.AtividadeAlternativaId == null && (int)l.NivelDificuldade <= nivelMaximo)
                        + u.AtividadesAlternativa.Count(a => (int)a.NivelDificuldade <= nivelMaximo)
                        + u.LicoesVibracao.Count,
                    QtdAtividadesFala = u.AtividadesFala.Count,
                    QtdVideos = u.LicoesVideo.Count,
                    QtdAlternativas = u.LicoesAlternativa.Count,
                    TotalLicoes =
                        u.LicoesFala.Count(l => (int)l.NivelDificuldade <= nivelMaximo)
                        + u.LicoesVideo.Count(l => (int)l.NivelDificuldade <= nivelMaximo)
                        + u.LicoesAlternativa.Count(l => (int)l.NivelDificuldade <= nivelMaximo)
                        + u.LicoesVibracao.Count
                })
                .ToListAsync();
        }

        /// <summary>
        /// Só os ids e níveis de tudo que existe em cada unidade, no mesmo formato de
        /// GET /api/Unidades mas sem frases, perguntas e vídeos. É o suficiente pra calcular
        /// o progresso (tela "Seu progresso") sem baixar megabytes de conteúdo.
        /// </summary>
        [HttpGet("itens")]
        [Authorize]
        public async Task<IActionResult> GetItensDasUnidades()
        {
            var itens = await _context.Unidades
                .AsNoTracking()
                .OrderBy(u => u.IdUnidade)
                .Select(u => new
                {
                    u.IdUnidade,
                    u.Nome,
                    // exercícios de grupo já vêm dentro do grupo; aqui só os avulsos
                    LicoesFala = u.LicoesFala.Where(l => l.AtividadeFalaId == null)
                        .Select(l => new { l.IdLicaoFala, l.NivelDificuldade }).ToList(),
                    AtividadesFala = u.AtividadesFala.Select(a => new
                    {
                        a.IdAtividadeFala,
                        a.NivelDificuldade,
                        Exercicios = a.Exercicios.Select(e => new { e.IdLicaoFala }).ToList()
                    }).ToList(),
                    LicoesAlternativa = u.LicoesAlternativa.Where(l => l.AtividadeAlternativaId == null)
                        .Select(l => new { l.IdLicaoAlternativa, l.NivelDificuldade }).ToList(),
                    AtividadesAlternativa = u.AtividadesAlternativa.Select(a => new
                    {
                        a.IdAtividadeAlternativa,
                        a.NivelDificuldade,
                        Exercicios = a.Exercicios.Select(e => new { e.IdLicaoAlternativa }).ToList()
                    }).ToList(),
                    LicoesVideo = u.LicoesVideo.Select(l => new { l.IdLicaoVideo, l.NivelDificuldade }).ToList()
                })
                .AsSplitQuery()
                .ToListAsync();

            return Ok(itens);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Unidade>> GetUnidade(int id)
        {
            var unidade = await _context.Unidades
                .Include(u => u.LicoesFala)
                .Include(u => u.LicoesVideo)
                .Include(u => u.LicoesAlternativa)
                .Include(u => u.LicoesVibracao)
                .Include(u => u.AtividadesFala).ThenInclude(af => af.Exercicios.OrderBy(e => e.Ordem))
                .Include(u => u.AtividadesAlternativa).ThenInclude(aa => aa.Exercicios.OrderBy(e => e.IdLicaoAlternativa))
                .AsSplitQuery().FirstOrDefaultAsync(u => u.IdUnidade == id);

            if (unidade == null)
            {
                return NotFound(new { message = "Unidade não encontrada." });
            }

            return unidade;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Unidade>> PostUnidade(UnidadeCriacaoDto dto)
        {
            var atividadeExiste = await _context.Atividades.AnyAsync(a => a.IdAtividade == dto.AtividadeId);
            if (!atividadeExiste)
            {
                return BadRequest(new { message = "A Atividade informada não existe." });
            }

            var novaUnidade = new Unidade
            {
                Nome = dto.Nome,
                AtividadeId = dto.AtividadeId
            };

            _context.Unidades.Add(novaUnidade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUnidade), new { id = novaUnidade.IdUnidade }, novaUnidade);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUnidade(int id, UnidadeCriacaoDto dto)
        {
            var unidadeBanco = await _context.Unidades.FindAsync(id);

            if (unidadeBanco == null)
            {
                return NotFound(new { message = "Unidade não encontrada." });
            }

            var atividadeExiste = await _context.Atividades.AnyAsync(a => a.IdAtividade == dto.AtividadeId);
            if (!atividadeExiste)
            {
                return BadRequest(new { message = "A Atividade informada não existe." });
            }

            unidadeBanco.Nome = dto.Nome;
            unidadeBanco.AtividadeId = dto.AtividadeId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnidadeExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUnidade(int id)
        {
            var unidade = await _context.Unidades
                .Include(u => u.AtividadesFala).ThenInclude(a => a.Exercicios)
                .FirstOrDefaultAsync(u => u.IdUnidade == id);
            if (unidade == null) return NotFound();

            // AtividadesFala usa FK restrita: precisam sair antes da unidade (com os exercicios)
            foreach (var grupo in unidade.AtividadesFala)
                _context.LicoesFala.RemoveRange(grupo.Exercicios);
            _context.AtividadesFala.RemoveRange(unidade.AtividadesFala);
            _context.Unidades.Remove(unidade);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Não é possível excluir: já existe progresso de alunos nesse conteúdo." });
            }

            return NoContent();
        }

        private bool UnidadeExists(int id)
        {
            return _context.Unidades.Any(e => e.IdUnidade == id);
        }
    }
}
