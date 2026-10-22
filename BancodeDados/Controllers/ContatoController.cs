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
        [Authorize(Roles = "Usuario,Profissional")]
        public async Task<IActionResult> PostMensagem(MensagemContatoCriacaoDto dto)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ehProfissional = User.FindFirstValue(ClaimTypes.Role) == "Profissional";

            var mensagem = new MensagemContato
            {
                UsuarioId = ehProfissional ? null : id,
                ProfissionalId = ehProfissional ? id : null,
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
                .OrderByDescending(m => m.DataEnvio)
                .Select(m => new
                {
                    m.IdMensagem,
                    NomeUsuario = m.Usuario != null ? m.Usuario.Nome : m.Profissional!.Nome,
                    EmailUsuario = m.Usuario != null ? m.Usuario.Email : m.Profissional!.Email,
                    Origem = m.Usuario != null ? "Aluno" : "Profissional",
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