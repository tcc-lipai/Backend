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

        private static readonly HashSet<string> TiposItemValidos = new() { "video", "alternativa", "fala", "fala-sessao", "unidade" };

        private async Task<bool> ItemExisteAsync(string tipoItem, int itemId) => tipoItem switch
        {
            "video" => await _context.LicoesVideo.AnyAsync(l => l.IdLicaoVideo == itemId),
            "alternativa" => await _context.LicoesAlternativa.AnyAsync(l => l.IdLicaoAlternativa == itemId),
            "fala" => await _context.LicoesFala.AnyAsync(l => l.IdLicaoFala == itemId),
            "fala-sessao" => await _context.AtividadesFala.AnyAsync(a => a.IdAtividadeFala == itemId),
            "unidade" => await _context.Unidades.AnyAsync(u => u.IdUnidade == itemId),
            _ => false
        };

        [HttpPost("item/salvar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> SalvarItem(ItemSalvoDto dto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!TiposItemValidos.Contains(dto.TipoItem))
                return BadRequest(new { message = "Tipo de item inválido." });

            if (!await ItemExisteAsync(dto.TipoItem, dto.ItemId))
                return BadRequest(new { message = "O item informado não existe." });

            var jaSalvo = await _context.ItensSalvos
                .AnyAsync(i => i.UsuarioId == usuarioId && i.TipoItem == dto.TipoItem && i.ItemId == dto.ItemId);
            if (jaSalvo)
                return BadRequest(new { message = "Esse item já está salvo." });

            _context.ItensSalvos.Add(new ItemSalvo
            {
                UsuarioId = usuarioId,
                TipoItem = dto.TipoItem,
                ItemId = dto.ItemId,
                DataSalva = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Esse item já está salvo." });
            }

            return Ok(new { message = "Item salvo com sucesso." });
        }

        [HttpDelete("item/salvar/{tipoItem}/{itemId}")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> DessalvarItem(string tipoItem, int itemId)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var salvo = await _context.ItensSalvos
                .FirstOrDefaultAsync(i => i.UsuarioId == usuarioId && i.TipoItem == tipoItem && i.ItemId == itemId);

            if (salvo == null) return NotFound(new { message = "Esse item não está salvo." });

            _context.ItensSalvos.Remove(salvo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("item/salvos")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> GetItensSalvos()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var salvos = await _context.ItensSalvos
                .Where(i => i.UsuarioId == usuarioId)
                .OrderByDescending(i => i.DataSalva)
                .ToListAsync();

            var resultado = new List<object>();
            foreach (var item in salvos)
            {
                string? titulo = item.TipoItem switch
                {
                    "video" => (await _context.LicoesVideo.FindAsync(item.ItemId))?.Titulo,
                    "alternativa" => (await _context.LicoesAlternativa.FindAsync(item.ItemId))?.Pergunta,
                    "fala" => (await _context.LicoesFala.FindAsync(item.ItemId))?.FraseEsperada,
                    "fala-sessao" => (await _context.AtividadesFala.FindAsync(item.ItemId))?.Nome,
                    "unidade" => (await _context.Unidades.FindAsync(item.ItemId))?.Nome,
                    _ => null
                };

                if (titulo == null) continue;

                resultado.Add(new
                {
                    item.TipoItem,
                    item.ItemId,
                    Titulo = titulo,
                    item.DataSalva
                });
            }

            return Ok(resultado);
        }
    }
}