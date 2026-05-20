using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutoController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. CADASTRAR PRODUTO NA LOJA (POST: api/Produto)
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(ProdutoCriacaoDto dto)
        {
            var novoProduto = new Produto
            {
                Nome = dto.Nome,
                Preco = dto.Preco
            };

            _context.Produtos.Add(novoProduto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProdutoPorId), new { id = novoProduto.IdProduto }, novoProduto);
        }

        // ==========================================
        // 2. COMPRAR PRODUTO (POST: api/Produto/comprar)
        // ==========================================
        [HttpPost("comprar")]
        public async Task<IActionResult> ComprarProduto(CompraProdutoDto dto)
        {
            // 1. Valida se o usuário existe
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            // 2. Valida se o produto existe
            var produto = await _context.Produtos.FindAsync(dto.ProdutoId);
            if (produto == null)
            {
                return NotFound(new { message = "Produto não encontrado." });
            }

            if (dto.Quantidade <= 0)
            {
                return BadRequest(new { message = "A quantidade deve ser maior que zero." });
            }

            // 3. Calcula o total da compra
            decimal total = produto.Preco * dto.Quantidade;

            // 4. Regra de Negócio: Valida se o usuário tem saldo suficiente
            if (usuario.SaldoAtual < total)
            {
                return BadRequest(new { message = $"Saldo insuficiente. O total é {total} e você possui {usuario.SaldoAtual}." });
            }

            // 5. Deduz o saldo do usuário
            usuario.SaldoAtual -= total;

            // 6. Registra a compra na tabela intermediária
            var historicoCompra = new ProdutoUsuario
            {
                UsuarioId = dto.UsuarioId,
                ProdutoId = dto.ProdutoId,
                Qtde = dto.Quantidade,
                TotalCompra = total,
                DataCompra = DateTime.UtcNow
            };
            _context.ProdutoUsuarios.Add(historicoCompra);

            // 🔥 DISPARO AUTOMÁTICO: Cria a notificação de sucesso da compra usando apenas 'Descricao'
            var novaNotificacao = new Notificacao
            {
                UsuarioId = dto.UsuarioId,
                Descricao = $"Compra realizada com sucesso! 🛍️ Você adquiriu {dto.Quantidade}x {produto.Nome} por um total de {total} moedas."
            };
            _context.Notificacoes.Add(novaNotificacao);

            // Salva todas as operações juntas de forma consistente
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Compra realizada com sucesso!",
                novoSaldo = usuario.SaldoAtual,
                totalGasto = total
            });
        }

        // ==========================================
        // 3. LISTAR PRODUTOS DA LOJA (GET: api/Produto)
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // ==========================================
        // 4. BUSCAR PRODUTO POR ID (GET: api/Produto/5)
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProdutoPorId(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            return produto;
        }

        // ==========================================
        // 5. ATUALIZAR PRODUTO (PUT: api/Produto/5)
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, ProdutoCriacaoDto dto)
        {
            var produtoBanco = await _context.Produtos.FindAsync(id);
            if (produtoBanco == null) return NotFound();

            produtoBanco.Nome = dto.Nome;
            produtoBanco.Preco = dto.Preco;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // 6. DELETAR PRODUTO DA LOJA (DELETE: api/Produto/5)
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}