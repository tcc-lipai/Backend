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
    public class LicaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LicaoController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // ALTERNATIVA (interpretação de leitura labial)
        // ==========================================
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
                ValorGanho = dto.ValorGanho
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

        // ==========================================
        // FALA
        // ==========================================
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
                ValorGanho = dto.ValorGanho
            };

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
            return Ok(licao);
        }

        [HttpPut("fala/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutFala(int id, LicaoFalaCriacaoDto dto)
        {
            var licao = await _context.LicoesFala.FindAsync(id);
            if (licao == null) return NotFound();

            licao.UnidadeId = dto.UnidadeId;
            licao.FraseEsperada = dto.FraseEsperada;
            licao.NivelDificuldade = dto.NivelDificuldade;
            licao.ValorGanho = dto.ValorGanho;

            await _context.SaveChangesAsync();
            return NoContent();
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

        // ==========================================
        // VIDEO (vídeo-aula, sem progressão/nota)
        // ==========================================
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
                VideoUrl = dto.VideoUrl
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

        // ==========================================
        // VIBRAÇÃO (acompanhante, sem progressão/nota)
        // ==========================================
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