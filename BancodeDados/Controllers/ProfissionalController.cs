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
    public class ProfissionalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfissionalController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarProfissional(ProfissionalCadastroDto dto)
        {
            var emailExiste = await _context.Profissionais.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower())
                || await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

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

            return CreatedAtAction(nameof(GetProfissionalPorId), new { id = profissional.IdProfissional }, new
            {
                profissional.IdProfissional,
                profissional.Nome,
                profissional.Email,
                profissional.CodigoAcesso
            });
        }

        [HttpGet]
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
                    QtdePacientes = p.Vinculos.Count
                })
                .ToListAsync();

            return Ok(profissionais);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProfissionalPorId(int id)
        {
            var profissional = await _context.Profissionais
                .Include(p => p.Vinculos).ThenInclude(v => v.Usuario)
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
                QtdePacientes = profissional.Vinculos.Count,
                Pacientes = profissional.Vinculos.Select(v => v.Usuario!).Select(u => new
                {
                    u.IdUsuario,
                    u.Nome,
                    u.Email,
                    u.NivelDificuldade,
                    u.Diagnostico
                })
            });
        }

        [HttpPut("{id}")]
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExcluirProfissional(int id)
        {
            var profissional = await _context.Profissionais.FindAsync(id);
            if (profissional == null)
                return NotFound(new { message = "Profissional não encontrado." });

            _context.Profissionais.Remove(profissional);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("pacientes")]
        [Authorize(Roles = "Profissional")]
        public async Task<IActionResult> GetPacientes()
        {
            var profissionalId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var pacientes = await _context.VinculosProfissional
                .Where(v => v.ProfissionalId == profissionalId)
                .Select(v => v.Usuario!)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nome,
                    u.Email,
                    u.NivelDificuldade,
                    u.Diagnostico,
                    u.UltimaAtividadeData
                })
                .ToListAsync();

            return Ok(pacientes);
        }

        private static string GerarCodigoAcesso()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}