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
    public class AtividadeAlternativaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AtividadeAlternativaController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Versao leve de um grupo de interpretacao: nome, nivel e so os ids dos
        /// exercicios, em ordem. Serve a sessao de interpretacao, que nao precisa
        /// carregar as 26 unidades so pra achar os exercicios de um grupo. Cada
        /// exercicio continua sendo buscado (e tendo o nivel checado) em GET
        /// /Licao/alternativa/{id}.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAtividadeAlternativa(int id)
        {
            var atividade = await _context.AtividadesAlternativa
                .Where(a => a.IdAtividadeAlternativa == id)
                .Select(a => new
                {
                    a.IdAtividadeAlternativa,
                    a.Nome,
                    a.NivelDificuldade,
                    a.UnidadeId,
                    Exercicios = a.Exercicios.OrderBy(e => e.IdLicaoAlternativa).Select(e => e.IdLicaoAlternativa).ToList()
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
        public async Task<IActionResult> PostAtividadeAlternativa(AtividadeAlternativaCriacaoDto dto)
        {
            if (!await _context.Unidades.AnyAsync(u => u.IdUnidade == dto.UnidadeId))
                return BadRequest(new { message = "A Unidade informada não existe." });

            var atividade = new AtividadeAlternativa
            {
                Nome = dto.Nome,
                NivelDificuldade = dto.NivelDificuldade,
                UnidadeId = dto.UnidadeId
            };

            _context.AtividadesAlternativa.Add(atividade);
            await _context.SaveChangesAsync();
            return Ok(new { atividade.IdAtividadeAlternativa });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAtividadeAlternativa(int id, AtividadeAlternativaCriacaoDto dto)
        {
            var atividade = await _context.AtividadesAlternativa.FindAsync(id);
            if (atividade == null) return NotFound();

            atividade.Nome = dto.Nome;
            atividade.NivelDificuldade = dto.NivelDificuldade;
            atividade.UnidadeId = dto.UnidadeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAtividadeAlternativa(int id)
        {
            var atividade = await _context.AtividadesAlternativa
                .Include(a => a.Exercicios)
                .FirstOrDefaultAsync(a => a.IdAtividadeAlternativa == id);
            if (atividade == null) return NotFound();

            // os exercicios pertencem ao grupo: sai tudo junto (senao virariam exercicios avulsos)
            _context.LicoesAlternativa.RemoveRange(atividade.Exercicios);
            _context.AtividadesAlternativa.Remove(atividade);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Não é possível excluir: já existe progresso de alunos nesse conteúdo." });
            }
            return NoContent();
        }
    }
}
