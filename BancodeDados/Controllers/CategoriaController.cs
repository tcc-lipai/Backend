using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Categoria>> PostCategoria(CategoriaCriacaoDto dto)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Nome.ToLower() == dto.Nome.ToLower());

            if (categoriaExiste)
            {
                return BadRequest(new { message = "Já existe uma categoria com este nome." });
            }

            var novaCategoria = new Categoria
            {
                Nome = dto.Nome
            };

            _context.Categorias.Add(novaCategoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoriaPorId), new { id = novaCategoria.IdCategoria }, novaCategoria);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            return await _context.Categorias
                .Include(c => c.Palavras)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Categoria>> GetCategoriaPorId(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Palavras)
                .FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null)
                return NotFound(new { message = "Categoria não encontrada." });

            return categoria;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategoria(int id, CategoriaCriacaoDto dto)
        {
            var categoriaBanco = await _context.Categorias.FindAsync(id);

            if (categoriaBanco == null)
            {
                return NotFound(new { message = "Categoria não encontrada." });
            }

            var nomeExiste = await _context.Categorias
                .AnyAsync(c => c.Nome.ToLower() == dto.Nome.ToLower() && c.IdCategoria != id);

            if (nomeExiste)
            {
                return BadRequest(new { message = "Já existe outra categoria com este nome." });
            }

            categoriaBanco.Nome = dto.Nome;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.IdCategoria == id);
        }
    }
}