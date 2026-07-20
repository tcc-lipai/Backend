using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Services;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, JwtService jwtService, IConfiguration config)
        {
            _context = context;
            _jwtService = jwtService;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var adminEmail = _config["Admin:Email"];
            var adminSenhaHash = _config["Admin:SenhaHash"];

            if (adminEmail != null && dto.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                if (adminSenhaHash == null || !PasswordHasher.Verify(dto.Senha, adminSenhaHash))
                    return Unauthorized(new { message = "E-mail ou senha incorretos." });

                var tokenAdmin = _jwtService.GerarToken(0, "Admin", adminEmail, "Admin");
                return Ok(new AuthResponseDto { Token = tokenAdmin, Role = "Admin", Id = 0, Nome = "Admin", Email = adminEmail });
            }

            var profissional = await _context.Profissionais
                .FirstOrDefaultAsync(p => p.Email.ToLower() == dto.Email.ToLower());

            if (profissional != null)
            {
                if (!PasswordHasher.Verify(dto.Senha, profissional.SenhaHash))
                    return Unauthorized(new { message = "E-mail ou senha incorretos." });

                var tokenProf = _jwtService.GerarToken(profissional.IdProfissional, profissional.Nome, profissional.Email, "Profissional");
                return Ok(new AuthResponseDto { Token = tokenProf, Role = "Profissional", Id = profissional.IdProfissional, Nome = profissional.Nome, Email = profissional.Email });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (usuario != null)
            {
                if (!PasswordHasher.Verify(dto.Senha, usuario.SenhaHash))
                    return Unauthorized(new { message = "E-mail ou senha incorretos." });

                var tokenUsuario = _jwtService.GerarToken(usuario.IdUsuario, usuario.Nome, usuario.Email, "Usuario");
                return Ok(new AuthResponseDto { Token = tokenUsuario, Role = "Usuario", Id = usuario.IdUsuario, Nome = usuario.Nome, Email = usuario.Email });
            }

            return Unauthorized(new { message = "E-mail ou senha incorretos." });
        }
    }
}