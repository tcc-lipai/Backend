using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public UsuarioController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // ==========================================
        // 1. CADASTRO (POST: api/Usuario)
        // ==========================================
        [HttpPost]
        public async Task<ActionResult> PostUsuario(UsuarioCadastroDto dto)
        {
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (emailExiste)
                return BadRequest(new { message = "Este e-mail já está em uso." });

            int? profissionalId = null;
            if (!string.IsNullOrWhiteSpace(dto.CodigoProfissional))
            {
                var profissional = await _context.Profissionais
                    .FirstOrDefaultAsync(p => p.CodigoAcesso == dto.CodigoProfissional);

                if (profissional == null)
                    return BadRequest(new { message = "Código de profissional inválido." });

                profissionalId = profissional.IdProfissional;
            }

            var novoUsuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = PasswordHasher.Hash(dto.Senha),
                NivelDificuldade = dto.NivelDificuldade,
                Diagnostico = dto.Diagnostico,
                ProfissionalId = profissionalId,
                SaldoAtual = 0
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuarioPorId), new { id = novoUsuario.IdUsuario }, new
            {
                novoUsuario.IdUsuario,
                novoUsuario.Nome,
                novoUsuario.Email,
                novoUsuario.NivelDificuldade,
                VinculadoAProfissional = profissionalId != null
            });
        }

        // ==========================================
        // 2. LOGIN (POST: api/Usuario/login)
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (usuario == null || !PasswordHasher.Verify(dto.Senha, usuario.SenhaHash))
                return Unauthorized(new { message = "E-mail ou senha incorretos." });

            var token = _jwtService.GerarToken(usuario.IdUsuario, usuario.Nome, usuario.Email, "Usuario");

            return Ok(new AuthResponseDto
            {
                Token = token,
                Role = "Usuario",
                Id = usuario.IdUsuario,
                Nome = usuario.Nome,
                Email = usuario.Email
            });
        }

        // ==========================================
        // 2b. VINCULAR A UM PROFISSIONAL (depois do cadastro)
        // ==========================================
        [HttpPost("vincular-profissional")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> VincularProfissional(VincularProfissionalDto dto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return NotFound();

            var profissional = await _context.Profissionais
                .FirstOrDefaultAsync(p => p.CodigoAcesso == dto.CodigoProfissional);

            if (profissional == null)
                return BadRequest(new { message = "Código de profissional inválido." });

            usuario.ProfissionalId = profissional.IdProfissional;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conta vinculada com sucesso." });
        }

        // ==========================================
        // 3. EDITAR PERFIL (PUT: api/Usuario/5)
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> PutUsuario(int id, UsuarioEdicaoDto dto)
        {
            var usuarioLogadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (usuarioLogadoId != id) return Forbid();

            var usuarioBanco = await _context.Usuarios.FindAsync(id);
            if (usuarioBanco == null)
                return NotFound(new { message = "Usuário não encontrado." });

            if (usuarioBanco.Email.ToLower() != dto.Email.ToLower())
            {
                var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IdUsuario != id);
                if (emailExiste)
                    return BadRequest(new { message = "Este e-mail já está em uso por outro usuário." });
            }

            usuarioBanco.Nome = dto.Nome;
            usuarioBanco.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Senha))
                usuarioBanco.SenhaHash = PasswordHasher.Hash(dto.Senha);
            usuarioBanco.Diagnostico = dto.Diagnostico;
            usuarioBanco.NivelDificuldade = dto.NivelDificuldade;

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
        // 4. LISTAR TODOS (GET: api/Usuario) — apenas Admin
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Notificacoes)
                .Include(u => u.Conquistas)
                .Include(u => u.ProdutoUsuarios)
                    .ThenInclude(pu => pu.Produto)
                .ToListAsync();

            var resultado = usuarios.Select(u => new
            {
                u.IdUsuario,
                u.SaldoAtual,
                u.NivelDificuldade,
                u.Nome,
                u.Email,
                u.Diagnostico,
                u.ProfissionalId,
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
        // Usuario só vê a si mesmo; Profissional só vê seus pacientes; Admin vê todos
        // ==========================================
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUsuarioPorId(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var usuario = await _context.Usuarios
                .Include(u => u.Notificacoes)
                .Include(u => u.Conquistas)
                .Include(u => u.ProdutoUsuarios)
                    .ThenInclude(pu => pu.Produto)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
                return NotFound(new { message = "Usuário não encontrado." });

            if (role == "Usuario" && idLogado != id)
                return Forbid();

            if (role == "Profissional" && usuario.ProfissionalId != idLogado)
                return Forbid();

            return Ok(new
            {
                usuario.IdUsuario,
                usuario.SaldoAtual,
                usuario.NivelDificuldade,
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
        // 6. DELETAR (DELETE: api/Usuario/5) — apenas Admin
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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