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

        // ==========================================
        // 1. CADASTRAR CATEGORIA (POST: api/Categorias)
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(CategoriaCriacaoDto dto)
        {
            // Evita cadastrar categorias com nomes duplicados (opcional, mas uma boa prática)
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

        // ==========================================
        // 2. LISTAR TODAS (GET: api/Categorias)
        // ==========================================
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            return await _context.Categorias
                .Include(c => c.Palavras)
                .ToListAsync();
        }

        // ==========================================
        // 3. BUSCAR POR ID (GET: api/Categorias/5)
        // ==========================================
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

        // ==========================================
        // 4. EDITAR CATEGORIA (PUT: api/Categorias/5)
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, CategoriaCriacaoDto dto)
        {
            var categoriaBanco = await _context.Categorias.FindAsync(id);

            if (categoriaBanco == null)
            {
                return NotFound(new { message = "Categoria não encontrada." });
            }

            // Valida duplicidade ao renomear
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

        // ==========================================
        // 5. DELETAR CATEGORIA (DELETE: api/Categorias/5)
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            // Dica de amigo: Se houver itens vinculados a essa categoria no dicionário,
            // o banco pode travar a deleção dependendo da sua configuração de Cascade.
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