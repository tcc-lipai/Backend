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
    public class LicaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LicaoController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checa se o usuário logado pode acessar um conteúdo com o nível dado
        /// (acesso cumulativo — ver RegraNivelService.PodeAcessarConteudo).
        /// Retorna null (libera) pra quem não é "Usuario" (Admin/Profissional).
        /// </summary>
        private async Task<IActionResult?> VerificarAcesso(NivelDificuldade nivelConteudo)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Usuario") return null;

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return NotFound();

            bool pode = RegraNivelService.PodeAcessarConteudo(usuario.NivelDificuldade, nivelConteudo);
            return pode ? null : Forbid();
        }

        [HttpPost("alternativa")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostAlternativa(LicaoAlternativaCriacaoDto dto)
        {
            var unidadeExiste = await _context.Unidades.AnyAsync(u => u.IdUnidade == dto.UnidadeId);
            if (!unidadeExiste)
                return BadRequest(new { message = "A Unidade informada não existe." });

            var licao = new LicaoAlternativa
            {
                UnidadeId = dto.UnidadeId,
                VideoUrl = dto.VideoUrl,
                Pergunta = dto.Pergunta,
                A = dto.A,
                B = dto.B,
                C = dto.C,
                D = dto.D,
                RespostaCerta = dto.RespostaCerta,
                ValorGanho = dto.ValorGanho,
                NivelDificuldade = dto.NivelDificuldade
            };

            _context.LicoesAlternativa.Add(licao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlternativaPorId), new { id = licao.IdLicaoAlternativa }, licao);
        }

        [HttpGet("alternativa/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAlternativaPorId(int id)
        {
            var licao = await _context.LicoesAlternativa.FindAsync(id);
            if (licao == null) return NotFound();

            var bloqueio = await VerificarAcesso(licao.NivelDificuldade);
            if (bloqueio != null) return bloqueio;

            return Ok(licao);
        }

        [HttpPut("alternativa/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAlternativa(int id, LicaoAlternativaCriacaoDto dto)
        {
            var licao = await _context.LicoesAlternativa.FindAsync(id);
            if (licao == null) return NotFound();

            licao.UnidadeId = dto.UnidadeId;
            licao.VideoUrl = dto.VideoUrl;
            licao.Pergunta = dto.Pergunta;
            licao.A = dto.A;
            licao.B = dto.B;
            licao.C = dto.C;
            licao.D = dto.D;
            licao.RespostaCerta = dto.RespostaCerta;
            licao.ValorGanho = dto.ValorGanho;
            licao.NivelDificuldade = dto.NivelDificuldade;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("alternativa/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAlternativa(int id)
        {
            var licao = await _context.LicoesAlternativa.FindAsync(id);
            if (licao == null) return NotFound();
            _context.LicoesAlternativa.Remove(licao);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("fala")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostFala(LicaoFalaCriacaoDto dto)
        {
            var unidadeExiste = await _context.Unidades.AnyAsync(u => u.IdUnidade == dto.UnidadeId);
            if (!unidadeExiste)
                return BadRequest(new { message = "A Unidade informada não existe." });

            var licao = new LicaoFala
            {
                UnidadeId = dto.UnidadeId,
                FraseEsperada = dto.FraseEsperada,
                NivelDificuldade = dto.NivelDificuldade,
                ValorGanho = dto.ValorGanho,
                AtividadeFalaId = dto.AtividadeFalaId
            };
            if (dto.AtividadeFalaId != null)
                licao.Ordem = await ProximaOrdemFala(dto.AtividadeFalaId.Value);

            _context.LicoesFala.Add(licao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFalaPorId), new { id = licao.IdLicaoFala }, licao);
        }

        [HttpGet("fala/{id}")]
        [Authorize]
        public async Task<IActionResult> GetFalaPorId(int id)
        {
            var licao = await _context.LicoesFala.FindAsync(id);
            if (licao == null) return NotFound();

            var bloqueio = await VerificarAcesso(licao.NivelDificuldade);
            if (bloqueio != null) return bloqueio;

            return Ok(licao);
        }

        [HttpPut("fala/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutFala(int id, LicaoFalaCriacaoDto dto)
        {
            var licao = await _context.LicoesFala.FindAsync(id);
            if (licao == null) return NotFound();

            if (dto.AtividadeFalaId != licao.AtividadeFalaId && dto.AtividadeFalaId != null)
                licao.Ordem = await ProximaOrdemFala(dto.AtividadeFalaId.Value);

            licao.UnidadeId = dto.UnidadeId;
            licao.FraseEsperada = dto.FraseEsperada;
            licao.NivelDificuldade = dto.NivelDificuldade;
            licao.ValorGanho = dto.ValorGanho;
            licao.AtividadeFalaId = dto.AtividadeFalaId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<int> ProximaOrdemFala(int atividadeFalaId)
        {
            var maior = await _context.LicoesFala
                .Where(l => l.AtividadeFalaId == atividadeFalaId)
                .Select(l => (int?)l.Ordem)
                .MaxAsync();
            return (maior ?? 0) + 1;
        }

        [HttpDelete("fala/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFala(int id)
        {
            var licao = await _context.LicoesFala.FindAsync(id);
            if (licao == null) return NotFound();
            _context.LicoesFala.Remove(licao);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("video")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostVideo(LicaoVideoCriacaoDto dto)
        {
            var unidadeExiste = await _context.Unidades.AnyAsync(u => u.IdUnidade == dto.UnidadeId);
            if (!unidadeExiste)
                return BadRequest(new { message = "A Unidade informada não existe." });

            var licao = new LicaoVideo
            {
                UnidadeId = dto.UnidadeId,
                Titulo = dto.Titulo,
                VideoDescricao = dto.VideoDescricao,
                VideoUrl = dto.VideoUrl,
                NivelDificuldade = dto.NivelDificuldade
            };

            _context.LicoesVideo.Add(licao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVideoPorId), new { id = licao.IdLicaoVideo }, licao);
        }

        [HttpGet("video/{id}")]
        [Authorize]
        public async Task<IActionResult> GetVideoPorId(int id)
        {
            var licao = await _context.LicoesVideo.FindAsync(id);
            if (licao == null) return NotFound();

            var bloqueio = await VerificarAcesso(licao.NivelDificuldade);
            if (bloqueio != null) return bloqueio;

            return Ok(licao);
        }

        [HttpPut("video/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutVideo(int id, LicaoVideoCriacaoDto dto)
        {
            var licao = await _context.LicoesVideo.FindAsync(id);
            if (licao == null) return NotFound();

            licao.UnidadeId = dto.UnidadeId;
            licao.Titulo = dto.Titulo;
            licao.VideoDescricao = dto.VideoDescricao;
            licao.VideoUrl = dto.VideoUrl;
            licao.NivelDificuldade = dto.NivelDificuldade;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("video/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var licao = await _context.LicoesVideo.FindAsync(id);
            if (licao == null) return NotFound();
            _context.LicoesVideo.Remove(licao);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("vibracao")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostVibracao(LicaoVibracaoCriacaoDto dto)
        {
            var unidadeExiste = await _context.Unidades.AnyAsync(u => u.IdUnidade == dto.UnidadeId);
            if (!unidadeExiste)
                return BadRequest(new { message = "A Unidade informada não existe." });

            var licao = new LicaoVibracao
            {
                UnidadeId = dto.UnidadeId,
                Silaba = dto.Silaba,
                Instrucao = dto.Instrucao
            };

            _context.LicoesVibracao.Add(licao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVibracaoPorId), new { id = licao.IdLicaoVibracao }, licao);
        }

        [HttpGet("vibracao/{id}")]
        [Authorize]
        public async Task<IActionResult> GetVibracaoPorId(int id)
        {
            // Vibração representa o nível 1 (Iniciante) e fica sempre liberada
            // pra qualquer usuário autenticado — ver RegraNivelService.PodeAcessarVibracao.

            var licao = await _context.LicoesVibracao.FindAsync(id);
            if (licao == null) return NotFound();
            return Ok(licao);
        }

        [HttpPut("vibracao/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutVibracao(int id, LicaoVibracaoCriacaoDto dto)
        {
            var licao = await _context.LicoesVibracao.FindAsync(id);
            if (licao == null) return NotFound();

            licao.UnidadeId = dto.UnidadeId;
            licao.Silaba = dto.Silaba;
            licao.Instrucao = dto.Instrucao;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("vibracao/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVibracao(int id)
        {
            var licao = await _context.LicoesVibracao.FindAsync(id);
            if (licao == null) return NotFound();
            _context.LicoesVibracao.Remove(licao);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}