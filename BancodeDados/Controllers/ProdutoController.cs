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
    public class ProdutoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutoController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. CADASTRAR PRODUTO NA LOJA (POST: api/Produto) — somente Admin
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Produto>> PostProduto(ProdutoCriacaoDto dto)
        {
            var novoProduto = new Produto
            {
                Nome = dto.Nome,
                Preco = dto.Preco,
                Tipo = dto.Tipo,
                DuracaoDias = dto.DuracaoDias,
                ValorMultiplicador = dto.ValorMultiplicador
            };

            _context.Produtos.Add(novoProduto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProdutoPorId), new { id = novoProduto.IdProduto }, novoProduto);
        }

        // ==========================================
        // 2. COMPRAR PRODUTO (POST: api/Produto/comprar) — Usuario
        // ==========================================
        [HttpPost("comprar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ComprarProduto(CompraProdutoDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
                return NotFound(new { message = "Usuário não encontrado." });

            var produto = await _context.Produtos.FindAsync(dto.ProdutoId);
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado." });

            if (dto.Quantidade <= 0)
                return BadRequest(new { message = "A quantidade deve ser maior que zero." });

            decimal total = produto.Preco * dto.Quantidade;

            if (usuario.SaldoAtual < total)
                return BadRequest(new { message = $"Saldo insuficiente. O total é {total} e você possui {usuario.SaldoAtual}." });

            usuario.SaldoAtual -= total;

            // Aplica o efeito do produto, se ele tiver um
            if (produto.Tipo == TipoProduto.BloqueioOfensiva && produto.DuracaoDias.HasValue)
            {
                var baseAtual = usuario.OfensivaCongeladaAte > DateTime.UtcNow
                    ? usuario.OfensivaCongeladaAte.Value
                    : DateTime.UtcNow;

                usuario.OfensivaCongeladaAte = baseAtual.AddDays(produto.DuracaoDias.Value * dto.Quantidade);
            }
            else if (produto.Tipo == TipoProduto.MultiplicadorMoedas && produto.DuracaoDias.HasValue)
            {
                var baseAtual = usuario.MultiplicadorMoedasAte > DateTime.UtcNow
                    ? usuario.MultiplicadorMoedasAte.Value
                    : DateTime.UtcNow;

                usuario.MultiplicadorMoedasAte = baseAtual.AddDays(produto.DuracaoDias.Value * dto.Quantidade);
                usuario.MultiplicadorMoedasValor = produto.ValorMultiplicador;
            }

            var historicoCompra = new ProdutoUsuario
            {
                UsuarioId = dto.UsuarioId,
                ProdutoId = dto.ProdutoId,
                Qtde = dto.Quantidade,
                TotalCompra = total,
                DataCompra = DateTime.UtcNow
            };
            _context.ProdutoUsuarios.Add(historicoCompra);

            _context.Notificacoes.Add(new Notificacao
            {
                UsuarioId = dto.UsuarioId,
                Descricao = $"Compra realizada com sucesso! 🛍️ Você adquiriu {dto.Quantidade}x {produto.Nome} por um total de {total} moedas."
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Compra realizada com sucesso!",
                novoSaldo = usuario.SaldoAtual,
                totalGasto = total,
                ofensivaCongeladaAte = usuario.OfensivaCongeladaAte,
                multiplicadorAte = usuario.MultiplicadorMoedasAte
            });
        }

        // ==========================================
        // 3. LISTAR PRODUTOS DA LOJA (GET: api/Produto)
        // ==========================================
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // ==========================================
        // 4. BUSCAR PRODUTO POR ID (GET: api/Produto/5)
        // ==========================================
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Produto>> GetProdutoPorId(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            return produto;
        }

        // ==========================================
        // 5. ATUALIZAR PRODUTO (PUT: api/Produto/5) — somente Admin
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduto(int id, ProdutoCriacaoDto dto)
        {
            var produtoBanco = await _context.Produtos.FindAsync(id);
            if (produtoBanco == null) return NotFound();

            produtoBanco.Nome = dto.Nome;
            produtoBanco.Preco = dto.Preco;
            produtoBanco.Tipo = dto.Tipo;
            produtoBanco.DuracaoDias = dto.DuracaoDias;
            produtoBanco.ValorMultiplicador = dto.ValorMultiplicador;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // 6. DELETAR PRODUTO DA LOJA (DELETE: api/Produto/5) — somente Admin
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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