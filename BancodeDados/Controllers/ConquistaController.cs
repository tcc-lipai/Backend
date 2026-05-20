using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ==========================================
        // 1. CADASTRAR CONQUISTA (POST: api/Conquista)
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Conquista>> PostConquista(ConquistaCriacaoDto dto)
        {
            // Valida se o usuário informado existe
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == dto.UsuarioId);
            if (!usuarioExiste)
            {
                return BadRequest(new { message = "O Usuário informado não existe." });
            }

            // Cria a nova conquista baseada no DTO
            var novaConquista = new Conquista
            {
                Nome = dto.Nome,
                UsuarioId = dto.UsuarioId
            };
            _context.Conquistas.Add(novaConquista);

            // 🔥 DISPARO AUTOMÁTICO: Cria a notificação usando apenas 'Descricao'
            var novaNotificacao = new Notificacao
            {
                UsuarioId = dto.UsuarioId,
                Descricao = $"Nova Conquista Desbloqueada! 🏆 Parabéns, você ganhou a conquista: {dto.Nome}."
            };
            _context.Notificacoes.Add(novaNotificacao);

            // Salva ambas as entidades juntas no banco de dados
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConquistaPorId), new { id = novaConquista.IdConquista }, novaConquista);
        }

        // ==========================================
        // 2. LISTAR TODAS (GET: api/Conquista)
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conquista>>> GetConquistas()
        {
            return await _context.Conquistas.ToListAsync();
        }

        // ==========================================
        // 3. BUSCAR POR ID (GET: api/Conquista/5)
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Conquista>> GetConquistaPorId(int id)
        {
            var conquista = await _context.Conquistas.FindAsync(id);

            if (conquista == null)
            {
                return NotFound(new { message = "Conquista não encontrada." });
            }

            return conquista;
        }

        // ==========================================
        // 4. EDITAR CONQUISTA (PUT: api/Conquista/5)
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConquista(int id, ConquistaCriacaoDto dto)
        {
            var conquistaBanco = await _context.Conquistas.FindAsync(id);

            if (conquistaBanco == null)
            {
                return NotFound(new { message = "Conquista não encontrada." });
            }

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == dto.UsuarioId);
            if (!usuarioExiste)
            {
                return BadRequest(new { message = "O Usuário informado não existe." });
            }

            conquistaBanco.Nome = dto.Nome;
            conquistaBanco.UsuarioId = dto.UsuarioId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConquistaExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // ==========================================
        // 5. DELETAR CONQUISTA (DELETE: api/Conquista/5)
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConquista(int id)
        {
            var conquista = await _context.Conquistas.FindAsync(id);
            if (conquista == null) return NotFound();

            _context.Conquistas.Remove(conquista);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConquistaExists(int id)
        {
            return _context.Conquistas.Any(e => e.IdConquista == id);
        }
    }
}