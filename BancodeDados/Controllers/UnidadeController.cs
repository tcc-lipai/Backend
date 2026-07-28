using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/Unidades
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Unidade>>> GetUnidades()
        {
            // Retorna as unidades incluindo os 3 tipos de lições que ela possui
            return await _context.Unidades
                .Include(u => u.LicoesFala)
                .Include(u => u.LicoesVideo)
                .Include(u => u.LicoesAlternativa)
                .ToListAsync();
        }

        // GET: api/Unidades/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Unidade>> GetUnidade(int id)
        {
            var unidade = await _context.Unidades
                .Include(u => u.LicoesFala)
                .Include(u => u.LicoesVideo)
                .Include(u => u.LicoesAlternativa)
                .FirstOrDefaultAsync(u => u.IdUnidade == id);

            if (unidade == null)
            {
                return NotFound(new { message = "Unidade não encontrada." });
            }

            return unidade;
        }

        // POST: api/Unidades
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Unidade>> PostUnidade(UnidadeCriacaoDto dto)
        {
            // 1. Valida se a Atividade informada realmente existe no banco
            var atividadeExiste = await _context.Atividades.AnyAsync(a => a.IdAtividade == dto.AtividadeId);
            if (!atividadeExiste)
            {
                return BadRequest(new { message = "A Atividade informada não existe." });
            }

            // 2. Mapeia o DTO simples para o Model de Unidade
            var novaUnidade = new Unidade
            {
                Nome = dto.Nome,
                AtividadeId = dto.AtividadeId
                // As listas de lições já iniciam vazias por padrão no seu Model
            };

            // 3. Salva no banco
            _context.Unidades.Add(novaUnidade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUnidade), new { id = novaUnidade.IdUnidade }, novaUnidade);
        }

        // PUT: api/Unidades/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUnidade(int id, UnidadeCriacaoDto dto)
        {
            var unidadeBanco = await _context.Unidades.FindAsync(id);

            if (unidadeBanco == null)
            {
                return NotFound(new { message = "Unidade não encontrada." });
            }

            // Opcional: Validar se a atividade existe caso queira permitir mudar a unidade de atividade
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

        // DELETE: api/Unidades/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUnidade(int id)
        {
            var unidade = await _context.Unidades.FindAsync(id);
            if (unidade == null) return NotFound();

            _context.Unidades.Remove(unidade);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UnidadeExists(int id)
        {
            return _context.Unidades.Any(e => e.IdUnidade == id);
        }
    }
}