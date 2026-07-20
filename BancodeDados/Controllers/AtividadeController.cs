using Backend.Data; // Ajuste para o namespace do seu DbContext
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/Atividades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Atividade>>> GetAtividades()
        {
            // Inclui as Unidades caso queira retornar a árvore completa
            return await _context.Atividades
                .Include(a => a.Unidades)
                .ToListAsync();
        }

        // GET: api/Atividades/5
        [HttpGet("{id}")]
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

        // POST: api/Atividades
        [HttpPost]
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


        // PUT: api/Atividades/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAtividade(int id, AtividadeCriacaoDto dto)
        {
            // 1. Busca a atividade direto do banco de dados
            var atividadeBanco = await _context.Atividades.FindAsync(id);

            if (atividadeBanco == null)
            {
                return NotFound(new { message = "Atividade não encontrada." });
            }

            // 2. Atualiza apenas os campos permitidos (no caso, o Nome)
            atividadeBanco.Nome = dto.Nome;
            atividadeBanco.Descricao = dto.Descricao;

            try
            {
                // 3. Salva as alterações
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

            return NoContent(); // Retorna 204 que deu tudo certo
        }

        // DELETE: api/Atividades/5
        [HttpDelete("{id}")]
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
    }
}