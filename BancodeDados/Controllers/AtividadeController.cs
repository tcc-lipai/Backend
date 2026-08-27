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
    public class AtividadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AtividadesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Atividade>>> GetAtividades()
        {
            return await _context.Atividades
                .Include(a => a.Unidades)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Atividade>> GetAtividade(int id)
        {
            var atividade = await _context.Atividades
                .Include(a => a.Unidades)
                .FirstOrDefaultAsync(a => a.IdAtividade == id);

            if (atividade == null)
            {
                return NotFound(new { message = "Atividade não encontrada." });
            }

            return atividade;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Atividade>> PostAtividade(AtividadeCriacaoDto dto)
        {
            var novaAtividade = new Atividade
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao
            };

            _context.Atividades.Add(novaAtividade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAtividade), new { id = novaAtividade.IdAtividade }, novaAtividade);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAtividade(int id, AtividadeCriacaoDto dto)
        {
            var atividadeBanco = await _context.Atividades.FindAsync(id);

            if (atividadeBanco == null)
            {
                return NotFound(new { message = "Atividade não encontrada." });
            }

            atividadeBanco.Nome = dto.Nome;
            atividadeBanco.Descricao = dto.Descricao;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AtividadeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAtividade(int id)
        {
            var atividade = await _context.Atividades.FindAsync(id);
            if (atividade == null)
            {
                return NotFound();
            }

            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AtividadeExists(int id)
        {
            return _context.Atividades.Any(e => e.IdAtividade == id);
        }

        [HttpPost("salvar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> SalvarAtividade(AtividadeSalvaDto dto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var atividadeExiste = await _context.Atividades.AnyAsync(a => a.IdAtividade == dto.AtividadeId);
            if (!atividadeExiste)
                return BadRequest(new { message = "A Atividade informada não existe." });

            var jaSalva = await _context.AtividadesSalvas
                .AnyAsync(a => a.UsuarioId == usuarioId && a.AtividadeId == dto.AtividadeId);
            if (jaSalva)
                return BadRequest(new { message = "Essa atividade já está salva." });

            _context.AtividadesSalvas.Add(new AtividadeSalva
            {
                UsuarioId = usuarioId,
                AtividadeId = dto.AtividadeId,
                DataSalva = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Atividade salva com sucesso." });
        }

        [HttpDelete("salvar/{atividadeId}")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> DessalvarAtividade(int atividadeId)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var salva = await _context.AtividadesSalvas
                .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.AtividadeId == atividadeId);

            if (salva == null) return NotFound(new { message = "Essa atividade não está salva." });

            _context.AtividadesSalvas.Remove(salva);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("salvas")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> GetAtividadesSalvas()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var salvas = await _context.AtividadesSalvas
                .Where(a => a.UsuarioId == usuarioId)
                .Include(a => a.Atividade)
                .Select(a => new
                {
                    a.Atividade!.IdAtividade,
                    a.Atividade.Nome,
                    a.Atividade.Descricao,
                    a.DataSalva
                })
                .ToListAsync();

            return Ok(salvas);
        }
    }
}