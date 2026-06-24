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
        // 1. CRIAR DICIONÁRIO PARA USUÁRIO (POST: api/Dicionario)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> PostDicionario(DicionarioCriacaoDto dto)
        {
            // Valida se o usuário existe
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == dto.UsuarioId);
            if (!usuarioExiste)
            {
                return BadRequest(new { message = "O Usuário informado não existe." });
            }

            // Valida se o usuário já possui um dicionário criado
            var jaPossuiDicionario = await _context.Dicionarios.AnyAsync(d => d.UsuarioId == dto.UsuarioId);
            if (jaPossuiDicionario)
            {
                return BadRequest(new { message = "Este usuário já possui um dicionário vinculado." });
            }

            var novoDicionario = new Dicionario
            {
                UsuarioId = dto.UsuarioId
            };

            _context.Dicionarios.Add(novoDicionario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Dicionário criado com sucesso!", idDicionario = novoDicionario.IdDicionario });
        }

        // ==========================================
        // 2. ADICIONAR ITEM AO DICIONÁRIO (POST: api/Dicionario/add-item)
        // ==========================================
        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem(ItemDicionarioCriacaoDto dto)
        {
            // 1. Valida se o dicionário de destino existe
            var dicionarioExiste = await _context.Dicionarios.AnyAsync(d => d.IdDicionario == dto.DicionarioId);
            if (!dicionarioExiste)
            {
                return BadRequest(new { message = "O Dicionário informado não existe." });
            }

            // 2. Valida se a categoria informada existe
            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.CategoriaId);
            if (!categoriaExiste)
            {
                return BadRequest(new { message = "A Categoria informada não existe." });
            }

            // 3. Evita duplicidade (opcional, mas evita o usuário adicionar o mesmo item duas vezes)
            var itemJaExiste = await _context.ItensDicionario.AnyAsync(i => i.DicionarioId == dto.DicionarioId && i.CategoriaId == dto.CategoriaId);
            if (itemJaExiste)
            {
                return BadRequest(new { message = "Esta categoria já foi adicionada a este dicionário." });
            }

            var novoItem = new ItemDicionario
            {
                DicionarioId = dto.DicionarioId,
                CategoriaId = dto.CategoriaId
            };

            _context.ItensDicionario.Add(novoItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item adicionado ao dicionário com sucesso!", idItemDicionario = novoItem.IdItemDicionario });
        }

        // ==========================================
        // 3. VER DICIONÁRIO POR ID DO USUÁRIO (GET: api/Dicionario/usuario/5)
        // ==========================================
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetDicionarioPorUsuario(int usuarioId)
        {
            // Traz o dicionário, a lista de itens dentro dele e inclui os detalhes da Categoria de cada item
            var dicionario = await _context.Dicionarios
                .Include(d => d.Itens)
                    .ThenInclude(i => i.Categoria)
                .FirstOrDefaultAsync(d => d.UsuarioId == usuarioId);

            if (dicionario == null)
            {
                return NotFound(new { message = "Dicionário não encontrado para este usuário." });
            }

            return Ok(dicionario);
        }

        // ==========================================
        // 4. REMOVER ITEM DO DICIONÁRIO (DELETE: api/Dicionario/remove-item/5)
        // ==========================================
        [HttpDelete("remove-item/{idItem}")]
        public async Task<IActionResult> DeleteItem(int idItem)
        {
            var item = await _context.ItensDicionario.FindAsync(idItem);
            if (item == null)
            {
                return NotFound(new { message = "Item não encontrado no dicionário." });
            }

            _context.ItensDicionario.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}