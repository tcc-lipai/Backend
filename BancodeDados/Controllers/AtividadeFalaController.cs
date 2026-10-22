using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using System.Security.Claims;

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

        /// <summary>
        /// Versao leve de um grupo de fala: nome, nivel e so os ids dos exercicios, na
        /// ordem. Serve a sessao de fala, que nao precisa carregar as 26 unidades so pra
        /// achar os exercicios de um grupo. Cada exercicio continua sendo buscado (e tendo
        /// o nivel checado) em GET /Licao/fala/{id}.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAtividadeFala(int id)
        {
            var atividade = await _context.AtividadesFala
                .Where(a => a.IdAtividadeFala == id)
                .Select(a => new
                {
                    a.IdAtividadeFala,
                    a.Nome,
                    a.NivelDificuldade,
                    a.UnidadeId,
                    Exercicios = a.Exercicios.OrderBy(e => e.Ordem).Select(e => e.IdLicaoFala).ToList()
                })
                .FirstOrDefaultAsync();
            if (atividade == null) return NotFound();

            if (User.FindFirstValue(ClaimTypes.Role) == "Usuario")
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null) return NotFound();
                if (!RegraNivelService.PodeAcessarConteudo(usuario.NivelDificuldade, atividade.NivelDificuldade))
                    return Forbid();
            }

            return Ok(atividade);
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
