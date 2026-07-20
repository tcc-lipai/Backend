using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Services;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfissionalController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public ProfissionalController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var profissional = await _context.Profissionais
                .FirstOrDefaultAsync(p => p.Email.ToLower() == dto.Email.ToLower());

            if (profissional == null || !PasswordHasher.Verify(dto.Senha, profissional.SenhaHash))
                return Unauthorized(new { message = "E-mail ou senha incorretos." });

            var token = _jwtService.GerarToken(profissional.IdProfissional, profissional.Nome, profissional.Email, "Profissional");

            return Ok(new AuthResponseDto
            {
                Token = token,
                Role = "Profissional",
                Id = profissional.IdProfissional,
                Nome = profissional.Nome,
                Email = profissional.Email
            });
        }

        // Tela "Seus Pacientes" do Figma
        [HttpGet("pacientes")]
        [Authorize(Roles = "Profissional")]
        public async Task<IActionResult> GetPacientes()
        {
            var profissionalId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var pacientes = await _context.Usuarios
                .Where(u => u.ProfissionalId == profissionalId)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nome,
                    u.NivelDificuldade,
                    u.Diagnostico
                })
                .ToListAsync();

            return Ok(pacientes);
        }
    }
}