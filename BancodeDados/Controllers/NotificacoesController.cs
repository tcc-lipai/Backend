using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var notificacoes = await _context.Notificacoes
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.IdNotificacao) // As mais recentes primeiro
                .ToListAsync();

            return Ok(notificacoes);
        }

        // ==========================================
        // 2. MARCAR COMO LIDA (PUT: api/Notificacoes/5)
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotificacao(int id, NotificacaoAtualizacaoDto dto)
        {
            var notificacao = await _context.Notificacoes.FindAsync(id);
            if (notificacao == null) return NotFound();

            // Geralmente usamos para mudar a propriedade 'Lida' para true
            // Se o seu model tiver uma propriedade com outro nome (ex: Visualizada, Status), mude aqui!
            // notificacao.Lida = dto.Lida; 

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}