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

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Unidade>>> GetUnidades()
        {
            return await _context.Unidades
                .Include(u => u.LicoesFala)
                .Include(u => u.LicoesVideo)
                .Include(u => u.LicoesAlternativa)
                .ToListAsync();
        }

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