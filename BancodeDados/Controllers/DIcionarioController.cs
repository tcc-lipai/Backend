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
    public class DicionarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DicionarioController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LISTAR TODAS AS PALAVRAS (GET: api/Dicionario)
        // Opcionalmente filtra por categoria: GET /api/Dicionario?categoriaId=2
        // ==========================================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPalavras([FromQuery] int? categoriaId)
        {
            var query = _context.Palavras.Include(p => p.Categoria).AsQueryable();

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            var palavras = await query
                .Select(p => new
                {
                    p.IdPalavra,
                    p.Nome,
                    p.VideoUrl,
                    p.CategoriaId,
                    NomeCategoria = p.Categoria!.Nome
                })
                .ToListAsync();

            return Ok(palavras);
        }

        // ==========================================
        // 2. BUSCAR PALAVRA POR ID (GET: api/Dicionario/5)
        // ==========================================
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPalavraPorId(int id)
        {
            var palavra = await _context.Palavras
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdPalavra == id);

            if (palavra == null)
                return NotFound(new { message = "Palavra não encontrada." });

            return Ok(new
            {
                palavra.IdPalavra,
                palavra.Nome,
                palavra.VideoUrl,
                palavra.CategoriaId,
                NomeCategoria = palavra.Categoria?.Nome
            });
        }

        // ==========================================
        // 3. CRIAR PALAVRA (POST: api/Dicionario) — somente Admin
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostPalavra(PalavraCriacaoDto dto)
        {
            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.CategoriaId);
            if (!categoriaExiste)
                return BadRequest(new { message = "A Categoria informada não existe." });

            var novaPalavra = new Palavra
            {
                Nome = dto.Nome,
                VideoUrl = dto.VideoUrl,
                CategoriaId = dto.CategoriaId
            };

            _context.Palavras.Add(novaPalavra);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPalavraPorId), new { id = novaPalavra.IdPalavra }, novaPalavra);
        }

        // ==========================================
        // 4. EDITAR PALAVRA (PUT: api/Dicionario/5) — somente Admin
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutPalavra(int id, PalavraCriacaoDto dto)
        {
            var palavra = await _context.Palavras.FindAsync(id);
            if (palavra == null)
                return NotFound(new { message = "Palavra não encontrada." });

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.CategoriaId);
            if (!categoriaExiste)
                return BadRequest(new { message = "A Categoria informada não existe." });

            palavra.Nome = dto.Nome;
            palavra.VideoUrl = dto.VideoUrl;
            palavra.CategoriaId = dto.CategoriaId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // 5. EXCLUIR PALAVRA (DELETE: api/Dicionario/5) — somente Admin
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePalavra(int id)
        {
            var palavra = await _context.Palavras.FindAsync(id);
            if (palavra == null) return NotFound();

            _context.Palavras.Remove(palavra);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}