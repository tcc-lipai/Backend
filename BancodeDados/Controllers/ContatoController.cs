using System.Security.Claims;
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
    [Authorize]
    public class ContatoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContatoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> PostMensagem(MensagemContatoCriacaoDto dto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var mensagem = new MensagemContato
            {
                UsuarioId = usuarioId,
                Mensagem = dto.Mensagem,
                DataEnvio = DateTime.UtcNow
            };

            _context.MensagensContato.Add(mensagem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mensagem enviada com sucesso!" });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMensagens()
        {
            var mensagens = await _context.MensagensContato
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.DataEnvio)
                .Select(m => new
                {
                    m.IdMensagem,
                    NomeUsuario = m.Usuario!.Nome,
                    EmailUsuario = m.Usuario.Email,
                    m.Mensagem,
                    m.DataEnvio,
                    m.Respondida
                })
                .ToListAsync();

            return Ok(mensagens);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarcarRespondida(int id)
        {
            var mensagem = await _context.MensagensContato.FindAsync(id);
            if (mensagem == null) return NotFound();

            mensagem.Respondida = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}