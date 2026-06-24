using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. CADASTRO (POST: api/Usuario)
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(UsuarioCadastroDto dto)
        {
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (emailExiste)
            {
                return BadRequest(new { message = "Este e-mail já está em uso." });
            }

            var novoUsuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Senha = dto.Senha,
                TipoUsuario = dto.TipoUsuario,
                Diagnostico = dto.Diagnostico,
                SaldoAtual = 0
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuarioPorId), new { id = novoUsuario.IdUsuario }, novoUsuario);
        }

        // ==========================================
        // 2. LOGIN (POST: api/Usuario/login)
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(UsuarioLoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (usuario == null || usuario.Senha != dto.Senha)
            {
                return Unauthorized(new { message = "E-mail ou senha incorretos." });
            }

            return Ok(new
            {
                message = "Login realizado com sucesso!",
                idUsuario = usuario.IdUsuario,
                nome = usuario.Nome,
                email = usuario.Email
            });
        }

        // ==========================================
        // 3. EDITAR PERFIL (PUT: api/Usuario/5)
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, UsuarioEdicaoDto dto)
        {
            var usuarioBanco = await _context.Usuarios.FindAsync(id);

            if (usuarioBanco == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            if (usuarioBanco.Email.ToLower() != dto.Email.ToLower())
            {
                var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IdUsuario != id);
                if (emailExiste)
                {
                    return BadRequest(new { message = "Este e-mail já está em uso por outro usuário." });
                }
            }

            usuarioBanco.Nome = dto.Nome;
            usuarioBanco.Email = dto.Email;
            usuarioBanco.Senha = dto.Senha;
            usuarioBanco.Diagnostico = dto.Diagnostico;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // ==========================================
        // 4. LISTAR TODOS (GET: api/Usuario)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            // Traz todos os usuários com suas respectivas compras e produtos mapeados de forma limpa
            var usuarios = await _context.Usuarios
                .Include(u => u.Notificacoes)
                .Include(u => u.Conquistas)
                .Include(u => u.ProdutoUsuarios)
                    .ThenInclude(pu => pu.Produto)
                .ToListAsync();

            // Mapeamento manual rápido para evitar o erro de ciclo infinito no JSON
            var resultado = usuarios.Select(u => new
            {
                u.IdUsuario,
                u.SaldoAtual,
                u.TipoUsuario,
                u.Nome,
                u.Email,
                u.Diagnostico,
                u.Notificacoes,
                u.Conquistas,
                ProdutosAdquiridos = u.ProdutoUsuarios.Select(pu => new
                {
                    pu.Id,
                    pu.ProdutoId,
                    NomeProduto = pu.Produto?.Nome,
                    PrecoUnitario = pu.Produto?.Preco,
                    pu.Qtde,
                    pu.TotalCompra,
                    pu.DataCompra
                })
            });

            return Ok(resultado);
        }

        // ==========================================
        // 5. BUSCAR POR ID (GET: api/Usuario/5)
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuarioPorId(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Notificacoes)
                .Include(u => u.Conquistas)
                .Include(u => u.ProdutoUsuarios)
                    .ThenInclude(pu => pu.Produto) // Carrega o produto de dentro da tabela intermediária
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            // Retorna um objeto customizado sem referências cíclicas para o Swagger ler perfeitamente
            return Ok(new
            {
                usuario.IdUsuario,
                usuario.SaldoAtual,
                usuario.TipoUsuario,
                usuario.Senha,
                usuario.Nome,
                usuario.Email,
                usuario.Diagnostico,
                usuario.Notificacoes,
                usuario.Conquistas,
                ProdutosAdquiridos = usuario.ProdutoUsuarios.Select(pu => new
                {
                    pu.Id,
                    pu.ProdutoId,
                    NomeProduto = pu.Produto?.Nome,
                    PrecoUnitario = pu.Produto?.Preco,
                    pu.Qtde,
                    pu.TotalCompra,
                    pu.DataCompra
                })
            });
        }

        // ==========================================
        // 6. DELETAR (DELETE: api/Usuario/5)
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}