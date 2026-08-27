using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConquistaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConquistaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetConquistas()
        {
            return Ok(await _context.Conquistas.ToListAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostConquista(ConquistaCriacaoDto dto)
        {
            var novaConquista = new Conquista
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                IconeUrl = dto.IconeUrl
            };

            _context.Conquistas.Add(novaConquista);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConquistaPorId), new { id = novaConquista.IdConquista }, novaConquista);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetConquistaPorId(int id)
        {
            var conquista = await _context.Conquistas.FindAsync(id);
            if (conquista == null) return NotFound();
            return Ok(conquista);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutConquista(int id, ConquistaCriacaoDto dto)
        {
            var conquista = await _context.Conquistas.FindAsync(id);
            if (conquista == null) return NotFound();

            conquista.Nome = dto.Nome;
            conquista.Descricao = dto.Descricao;
            conquista.IconeUrl = dto.IconeUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConquista(int id)
        {
            var conquista = await _context.Conquistas.FindAsync(id);
            if (conquista == null) return NotFound();

            _context.Conquistas.Remove(conquista);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("conceder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConcederConquista(ConcederConquistaDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
                return BadRequest(new { message = "O Usuário informado não existe." });

            var conquista = await _context.Conquistas.FindAsync(dto.ConquistaId);
            if (conquista == null)
                return BadRequest(new { message = "A Conquista informada não existe." });

            var jaTem = await _context.ConquistasUsuario
                .AnyAsync(cu => cu.UsuarioId == dto.UsuarioId && cu.ConquistaId == dto.ConquistaId);
            if (jaTem)
                return BadRequest(new { message = "O usuário já possui essa conquista." });

            _context.ConquistasUsuario.Add(new ConquistaUsuario
            {
                UsuarioId = dto.UsuarioId,
                ConquistaId = dto.ConquistaId,
                DataConquista = DateTime.UtcNow
            });

            _context.Notificacoes.Add(new Notificacao
            {
                UsuarioId = dto.UsuarioId,
                Descricao = $"Nova Conquista Desbloqueada! 🏆 Parabéns, você ganhou a conquista: {conquista.Nome}."
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Conquista concedida com sucesso." });
        }

        [HttpGet("usuario/{usuarioId}")]
        [Authorize]
        public async Task<IActionResult> GetConquistasUsuario(int usuarioId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Usuario" && idLogado != usuarioId) return Forbid();
            if (role == "Profissional")
            {
                var ehPaciente = await _context.Usuarios
                    .AnyAsync(u => u.IdUsuario == usuarioId && u.ProfissionalId == idLogado);
                if (!ehPaciente) return Forbid();
            }

            var todasConquistas = await _context.Conquistas.ToListAsync();

            var idsConquistadas = await _context.ConquistasUsuario
                .Where(cu => cu.UsuarioId == usuarioId)
                .ToDictionaryAsync(cu => cu.ConquistaId, cu => cu.DataConquista);

            var atingidas = todasConquistas
                .Where(c => idsConquistadas.ContainsKey(c.IdConquista))
                .Select(c => new { c.IdConquista, c.Nome, c.Descricao, c.IconeUrl, DataConquista = idsConquistadas[c.IdConquista] });

            var naoAtingidas = todasConquistas
                .Where(c => !idsConquistadas.ContainsKey(c.IdConquista))
                .Select(c => new { c.IdConquista, c.Nome, c.Descricao, c.IconeUrl });

            return Ok(new { Atingidas = atingidas, NaoAtingidas = naoAtingidas });
        }
    }
}