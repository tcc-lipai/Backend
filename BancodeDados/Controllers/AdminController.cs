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
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;

        public AdminController(AppDbContext context, JwtService jwtService, IConfiguration config)
        {
            _context = context;
            _jwtService = jwtService;
            _config = config;
        }

        // ==========================================
        // LOGIN DO ADMIN (conta fixa via config)
        // ==========================================
        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var adminEmail = _config["Admin:Email"];
            var adminSenhaHash = _config["Admin:SenhaHash"];

            if (adminEmail == null || adminSenhaHash == null)
                return StatusCode(500, new { message = "Admin não configurado." });

            if (!dto.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) ||
                !PasswordHasher.Verify(dto.Senha, adminSenhaHash))
            {
                return Unauthorized(new { message = "E-mail ou senha incorretos." });
            }

            var token = _jwtService.GerarToken(0, "Admin", adminEmail, "Admin");

            return Ok(new AuthResponseDto
            {
                Token = token,
                Role = "Admin",
                Id = 0,
                Nome = "Admin",
                Email = adminEmail
            });
        }

        // ==========================================
        // CRIAR PROFISSIONAL (somente Admin)
        // ==========================================
        [HttpPost("profissionais")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarProfissional(ProfissionalCadastroDto dto)
        {
            var emailExiste = await _context.Profissionais.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower());
            if (emailExiste)
                return BadRequest(new { message = "Este e-mail já está em uso." });

            string codigo;
            do
            {
                codigo = GerarCodigoAcesso();
            } while (await _context.Profissionais.AnyAsync(p => p.CodigoAcesso == codigo));

            var profissional = new Profissional
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = PasswordHasher.Hash(dto.Senha),
                Registro = dto.Registro,
                CodigoAcesso = codigo
            };

            _context.Profissionais.Add(profissional);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                profissional.IdProfissional,
                profissional.Nome,
                profissional.Email,
                profissional.CodigoAcesso
            });
        }

        // ==========================================
        // LISTAR PROFISSIONAIS (somente Admin)
        // ==========================================
        [HttpGet("profissionais")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProfissionais()
        {
            var profissionais = await _context.Profissionais
                .Select(p => new
                {
                    p.IdProfissional,
                    p.Nome,
                    p.Email,
                    p.Registro,
                    p.CodigoAcesso,
                    QtdePacientes = p.Pacientes.Count
                })
                .ToListAsync();

            return Ok(profissionais);
        }

        private static string GerarCodigoAcesso()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem 0/O/1/I pra evitar confusão
            var random = new Random();
            return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        // ==========================================
        // CRIAR USUARIO (Admin cadastra um usuário comum)
        // ==========================================
        [HttpPost("usuarios")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarUsuario(UsuarioCadastroDto dto)
        {
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower())
                || await _context.Profissionais.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower());

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

            return Ok(new
            {
                novoUsuario.IdUsuario,
                novoUsuario.Nome,
                novoUsuario.Email,
                novoUsuario.NivelDificuldade,
                VinculadoAProfissional = profissionalId != null
            });
        }

        // ==========================================
        // EDITAR USUARIO (Admin edita qualquer usuário)
        // ==========================================
        [HttpPut("usuarios/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarUsuario(int id, UsuarioEdicaoAdminDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { message = "Usuário não encontrado." });

            if (usuario.Email.ToLower() != dto.Email.ToLower())
            {
                var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IdUsuario != id)
                    || await _context.Profissionais.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower());

                if (emailExiste)
                    return BadRequest(new { message = "Este e-mail já está em uso." });
            }

            usuario.Nome = dto.Nome;
            usuario.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Senha))
                usuario.SenhaHash = PasswordHasher.Hash(dto.Senha);
            usuario.Diagnostico = dto.Diagnostico;
            usuario.NivelDificuldade = dto.NivelDificuldade;

            if (dto.CodigoProfissional == "")
            {
                usuario.ProfissionalId = null; // desvincula
            }
            else if (!string.IsNullOrWhiteSpace(dto.CodigoProfissional))
            {
                var profissional = await _context.Profissionais
                    .FirstOrDefaultAsync(p => p.CodigoAcesso == dto.CodigoProfissional);

                if (profissional == null)
                    return BadRequest(new { message = "Código de profissional inválido." });

                usuario.ProfissionalId = profissional.IdProfissional;
            }
            // se vier null, não mexe no vínculo atual

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // BUSCAR PROFISSIONAL POR ID (pra tela de edição)
        // ==========================================
        [HttpGet("profissionais/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProfissionalPorId(int id)
        {
            var profissional = await _context.Profissionais
                .Include(p => p.Pacientes)
                .FirstOrDefaultAsync(p => p.IdProfissional == id);

            if (profissional == null)
                return NotFound(new { message = "Profissional não encontrado." });

            return Ok(new
            {
                profissional.IdProfissional,
                profissional.Nome,
                profissional.Email,
                profissional.Registro,
                profissional.CodigoAcesso,
                QtdePacientes = profissional.Pacientes.Count
            });
        }

        // ==========================================
        // EDITAR PROFISSIONAL
        // ==========================================
        [HttpPut("profissionais/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarProfissional(int id, ProfissionalEdicaoDto dto)
        {
            var profissional = await _context.Profissionais.FindAsync(id);
            if (profissional == null)
                return NotFound(new { message = "Profissional não encontrado." });

            if (profissional.Email.ToLower() != dto.Email.ToLower())
            {
                var emailExiste = await _context.Profissionais.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower() && p.IdProfissional != id)
                    || await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

                if (emailExiste)
                    return BadRequest(new { message = "Este e-mail já está em uso." });
            }

            profissional.Nome = dto.Nome;
            profissional.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Senha))
                profissional.SenhaHash = PasswordHasher.Hash(dto.Senha);
            profissional.Registro = dto.Registro;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // EXCLUIR PROFISSIONAL
        // ==========================================
        [HttpDelete("profissionais/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExcluirProfissional(int id)
        {
            var profissional = await _context.Profissionais.FindAsync(id);
            if (profissional == null)
                return NotFound(new { message = "Profissional não encontrado." });

            _context.Profissionais.Remove(profissional);
            await _context.SaveChangesAsync();

            // Os pacientes vinculados a ele NÃO são apagados — o ProfissionalId deles
            // vira null automaticamente (já configuramos DeleteBehavior.SetNull no AppDbContext)
            return NoContent();
        }
    }
}