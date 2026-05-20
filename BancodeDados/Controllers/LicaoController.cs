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
        // 1. CREATE (POST) - COMPUTANDO PONTOS E NOTIFICAÇÃO
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> PostLicao(LicaoCriacaoDto dto)
        {
            var unidadeExiste = await _context.Unidades.AnyAsync(u => u.IdUnidade == dto.UnidadeId);
            if (!unidadeExiste)
            {
                return BadRequest(new { message = "A Unidade informada não existe." });
            }

            // Buscamos o usuário para poder incrementar o saldo dele após a lição
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
            {
                return BadRequest(new { message = "O Usuário informado não existe para computar a recompensa." });
            }

            switch (dto.TipoLicao.ToLower())
            {
                case "alternativa":
                    int valorAlternativa = 10;

                    var licaoAlt = new LicaoAlternativa
                    {
                        UnidadeId = dto.UnidadeId,
                        Pergunta = dto.Pergunta,
                        A = dto.A,
                        B = dto.B,
                        C = dto.C,
                        D = dto.D,
                        RespostaCerta = dto.RespostaCerta,
                        UsuarioId = dto.UsuarioId,
                        DataInicio = DateTime.UtcNow,
                        ValorGanho = valorAlternativa
                    };
                    _context.LicoesAlternativa.Add(licaoAlt);

                    // Atualiza saldo do usuário e gera notificação automática
                    usuario.SaldoAtual += valorAlternativa;
                    _context.Notificacoes.Add(new Notificacao
                    {
                        UsuarioId = dto.UsuarioId,
                        Descricao = $"Lição Concluída! 🪙 Você completou um exercício de alternativa e ganhou {valorAlternativa} moedas."
                    });

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Lição de Alternativa concluída com sucesso!", id = licaoAlt.IdLicaoAlternativa, tipo = "alternativa", novoSaldo = usuario.SaldoAtual });

                case "fala":
                    int valorFala = 15;

                    var licaoFala = new LicaoFala
                    {
                        UnidadeId = dto.UnidadeId,
                        FraseEsperada = dto.FraseEsperada,
                        DataInicio = DateTime.UtcNow,
                        ValorGanho = valorFala
                    };
                    _context.LicoesFala.Add(licaoFala);

                    // Atualiza saldo do usuário e gera notificação automática
                    usuario.SaldoAtual += valorFala;
                    _context.Notificacoes.Add(new Notificacao
                    {
                        UsuarioId = dto.UsuarioId,
                        Descricao = $"Lição Concluída! 🗣️ Você completou o exercício de fala e ganhou {valorFala} moedas."
                    });

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Lição de Fala concluída com sucesso!", id = licaoFala.IdLicaoFala, tipo = "fala", novoSaldo = usuario.SaldoAtual });

                case "video":
                    int valorVideo = 20;

                    var licaoVideo = new LicaoVideo
                    {
                        UnidadeId = dto.UnidadeId,
                        Titulo = dto.Titulo,
                        VideoDescricao = dto.VideoDescricao,
                        DataInicio = DateTime.UtcNow,
                        ValorGanho = valorVideo
                    };
                    _context.LicoesVideo.Add(licaoVideo);

                    // Atualiza saldo do usuário e gera notificação automática
                    usuario.SaldoAtual += valorVideo;
                    _context.Notificacoes.Add(new Notificacao
                    {
                        UsuarioId = dto.UsuarioId,
                        Descricao = $"Lição Concluída! 🎬 Você assistiu ao vídeo informativo e ganhou {valorVideo} moedas."
                    });

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Lição de Vídeo concluída com sucesso!", id = licaoVideo.IdLicaoVideo, tipo = "video", novoSaldo = usuario.SaldoAtual });

                default:
                    return BadRequest(new { message = "Tipo de lição inválido. Escolha entre: Alternativa, Fala ou Video." });
            }
        }

        // ==========================================
        // 2. READ ALL (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetLicoes()
        {
            var alternativas = await _context.LicoesAlternativa.ToListAsync();
            var falas = await _context.LicoesFala.ToListAsync();
            var videos = await _context.LicoesVideo.ToListAsync();

            return Ok(new
            {
                Alternativas = alternativas,
                Falas = falas,
                Videos = videos
            });
        }

        // ==========================================
        // 3. READ BY ID & TYPE (GET /api/Licao/fala/5)
        // ==========================================
        [HttpGet("{tipo}/{id}")]
        public async Task<IActionResult> GetLicaoPorId(string tipo, int id)
        {
            switch (tipo.ToLower())
            {
                case "alternativa":
                    var alt = await _context.LicoesAlternativa.FindAsync(id);
                    if (alt == null) return NotFound(new { message = "Lição de alternativa não encontrada." });
                    return Ok(alt);

                case "fala":
                    var fala = await _context.LicoesFala.FindAsync(id);
                    if (fala == null) return NotFound(new { message = "Lição de fala não encontrada." });
                    return Ok(fala);

                case "video":
                    var video = await _context.LicoesVideo.FindAsync(id);
                    if (video == null) return NotFound(new { message = "Lição de vídeo não encontrada." });
                    return Ok(video);

                default:
                    return BadRequest(new { message = "Tipo inválido." });
            }
        }

        // ==========================================
        // 4. UPDATE (PUT /api/Licao/video/5)
        // ==========================================
        [HttpPut("{tipo}/{id}")]
        public async Task<IActionResult> PutLicao(string tipo, int id, LicaoCriacaoDto dto)
        {
            switch (tipo.ToLower())
            {
                case "alternativa":
                    var alt = await _context.LicoesAlternativa.FindAsync(id);
                    if (alt == null) return NotFound(new { message = "Lição não encontrada." });

                    alt.Pergunta = dto.Pergunta;
                    alt.A = dto.A;
                    alt.B = dto.B;
                    alt.C = dto.C;
                    alt.D = dto.D;
                    alt.RespostaCerta = dto.RespostaCerta;
                    alt.UnidadeId = dto.UnidadeId;
                    break;

                case "fala":
                    var fala = await _context.LicoesFala.FindAsync(id);
                    if (fala == null) return NotFound(new { message = "Lição não encontrada." });

                    fala.FraseEsperada = dto.FraseEsperada;
                    fala.UnidadeId = dto.UnidadeId;
                    break;

                case "video":
                    var video = await _context.LicoesVideo.FindAsync(id);
                    if (video == null) return NotFound(new { message = "Lição não encontrada." });

                    video.Titulo = dto.Titulo;
                    video.VideoDescricao = dto.VideoDescricao;
                    video.UnidadeId = dto.UnidadeId;
                    break;

                default:
                    return BadRequest(new { message = "Tipo inválido." });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // 5. DELETE (DELETE /api/Licao/alternativa/5)
        // ==========================================
        [HttpDelete("{tipo}/{id}")]
        public async Task<IActionResult> DeleteLicao(string tipo, int id)
        {
            switch (tipo.ToLower())
            {
                case "alternativa":
                    var alt = await _context.LicoesAlternativa.FindAsync(id);
                    if (alt == null) return NotFound();
                    _context.LicoesAlternativa.Remove(alt);
                    break;

                case "fala":
                    var fala = await _context.LicoesFala.FindAsync(id);
                    if (fala == null) return NotFound();
                    _context.LicoesFala.Remove(fala);
                    break;

                case "video":
                    var video = await _context.LicoesVideo.FindAsync(id);
                    if (video == null) return NotFound();
                    _context.LicoesVideo.Remove(video);
                    break;

                default:
                    return BadRequest(new { message = "Tipo inválido." });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}