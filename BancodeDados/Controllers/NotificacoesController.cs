using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Backend.Data;
using Backend.Dtos;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificacoesController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. BUSCAR NOTIFICAÇÕES DO USUÁRIO (GET: api/Notificacoes/usuario/5)
        // ==========================================
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetNotificacoesPorUsuario(int usuarioId)
        {
            var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Usuario" && idLogado != usuarioId)
                return Forbid();

            if (role == "Profissional")
            {
                var ehPaciente = await _context.Usuarios
                    .AnyAsync(u => u.IdUsuario == usuarioId && u.ProfissionalId == idLogado);
                if (!ehPaciente) return Forbid();
            }

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();

            return Ok(notificacoes);
        }

        // ==========================================
        // 2. MARCAR COMO LIDA (PUT: api/Notificacoes/5)
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> PutNotificacao(int id, NotificacaoAtualizacaoDto dto)
        {
            var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var notificacao = await _context.Notificacoes.FindAsync(id);
            if (notificacao == null) return NotFound();

            if (notificacao.UsuarioId != idLogado)
                return Forbid();

            notificacao.Lida = dto.Lida;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}