using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtividadeFalaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AtividadeFalaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostAtividadeFala(AtividadeFalaCriacaoDto dto)
        {
            if (!await _context.Unidades.AnyAsync(u => u.IdUnidade == dto.UnidadeId))
                return BadRequest(new { message = "A Unidade informada não existe." });

            var atividade = new AtividadeFala
            {
                Nome = dto.Nome,
                NivelDificuldade = dto.NivelDificuldade,
                UnidadeId = dto.UnidadeId
            };

            _context.AtividadesFala.Add(atividade);
            await _context.SaveChangesAsync();
            return Ok(new { atividade.IdAtividadeFala });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAtividadeFala(int id, AtividadeFalaCriacaoDto dto)
        {
            var atividade = await _context.AtividadesFala.FindAsync(id);
            if (atividade == null) return NotFound();

            atividade.Nome = dto.Nome;
            atividade.NivelDificuldade = dto.NivelDificuldade;
            atividade.UnidadeId = dto.UnidadeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAtividadeFala(int id)
        {
            var atividade = await _context.AtividadesFala.FindAsync(id);
            if (atividade == null) return NotFound();

            _context.AtividadesFala.Remove(atividade);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
