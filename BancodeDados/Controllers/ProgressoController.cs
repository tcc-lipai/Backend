using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgressoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        private const double LimiarAcertoFala = 70.0;

        public ProgressoController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private int UsuarioLogadoId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("alternativa/{licaoId}/iniciar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> IniciarAlternativa(int licaoId)
        {
            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            if (!RegraNivelService.PodeAcessarOutrosTipos(usuario.NivelDificuldade))
                return Forbid();

            var licao = await _context.LicoesAlternativa.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "Lição não encontrada." });

            var progressoExistente = await _context.ProgressosAlternativa
                .FirstOrDefaultAsync(p => p.UsuarioId == UsuarioLogadoId
                    && p.LicaoAlternativaId == licaoId
                    && p.Status == StatusProgresso.EmAndamento);

            if (progressoExistente != null)
                return Ok(new { progressoExistente.IdProgresso });

            var progresso = new ProgressoAlternativa
            {
                UsuarioId = UsuarioLogadoId,
                LicaoAlternativaId = licaoId,
                Status = StatusProgresso.EmAndamento,
                DataInicio = DateTime.UtcNow
            };

            _context.ProgressosAlternativa.Add(progresso);
            await _context.SaveChangesAsync();

            return Ok(new { progresso.IdProgresso });
        }

        [HttpPost("alternativa/{idProgresso}/concluir")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConcluirAlternativa(int idProgresso, ResponderAlternativaDto dto)
        {
            var progresso = await _context.ProgressosAlternativa
                .Include(p => p.LicaoAlternativa)
                .FirstOrDefaultAsync(p => p.IdProgresso == idProgresso);

            if (progresso == null) return NotFound(new { message = "Progresso não encontrado." });
            if (progresso.UsuarioId != UsuarioLogadoId) return Forbid();
            if (progresso.Status == StatusProgresso.Concluido)
                return BadRequest(new { message = "Essa tentativa já foi concluída." });

            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);

            bool correta = progresso.LicaoAlternativa!.RespostaCerta
                .Equals(dto.RespostaDada, StringComparison.OrdinalIgnoreCase);

            progresso.RespostaDada = dto.RespostaDada;
            progresso.Correta = correta;
            progresso.Status = StatusProgresso.Concluido;
            progresso.DataConclusao = DateTime.UtcNow;

            AtualizarOfensiva(usuario!);

            if (correta)
            {
                var valorFinal = AplicarMultiplicador(usuario!, progresso.LicaoAlternativa.ValorGanho);
                progresso.PontuacaoObtida = valorFinal;
                usuario!.SaldoAtual += valorFinal;

                _context.Notificacoes.Add(new Notificacao
                {
                    UsuarioId = UsuarioLogadoId,
                    Descricao = $"Lição concluída! 🎉 Você acertou e ganhou {valorFinal} moedas."
                });
            }
            else
            {
                progresso.PontuacaoObtida = 0;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                progresso.Correta,
                progresso.PontuacaoObtida,
                NovoSaldo = usuario!.SaldoAtual
            });
        }

        [HttpPost("fala/{licaoId}/iniciar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> IniciarFala(int licaoId)
        {

            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            if (!RegraNivelService.PodeAcessarOutrosTipos(usuario.NivelDificuldade))
                return Forbid();

            var licao = await _context.LicoesFala.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "Lição não encontrada." });

            var progressoExistente = await _context.ProgressosFala
                .FirstOrDefaultAsync(p => p.UsuarioId == UsuarioLogadoId
                    && p.LicaoFalaId == licaoId
                    && p.Status == StatusProgresso.EmAndamento);

            if (progressoExistente != null)
                return Ok(new { progressoExistente.IdProgresso });

            var progresso = new ProgressoFala
            {
                UsuarioId = UsuarioLogadoId,
                LicaoFalaId = licaoId,
                Status = StatusProgresso.EmAndamento,
                DataInicio = DateTime.UtcNow
            };

            _context.ProgressosFala.Add(progresso);
            await _context.SaveChangesAsync();

            return Ok(new { progresso.IdProgresso });
        }

        [HttpPost("fala/{idProgresso}/concluir")]
        [Authorize(Roles = "Usuario")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ConcluirFala(int idProgresso, IFormFile arquivo)
        {
            var progresso = await _context.ProgressosFala
                .Include(p => p.LicaoFala)
                .FirstOrDefaultAsync(p => p.IdProgresso == idProgresso);

            if (progresso == null) return NotFound(new { message = "Progresso não encontrado." });
            if (progresso.UsuarioId != UsuarioLogadoId) return Forbid();
            if (progresso.Status == StatusProgresso.Concluido)
                return BadRequest(new { message = "Essa tentativa já foi concluída." });

            if (arquivo == null || arquivo.Length == 0)
                return BadRequest(new { message = "Nenhum arquivo de áudio enviado." });

            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);

            AvaliacaoFalaIaDto avaliacao;
            try
            {
                avaliacao = await AvaliarAudioComIa(progresso.LicaoFala!.FraseEsperada, arquivo);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Não foi possível avaliar o áudio — o serviço de IA está indisponível." });
            }

            bool correta = avaliacao.ScoreGeral >= LimiarAcertoFala;

            progresso.TextoReconhecidoPelaIA = avaliacao.TextoFalado;
            progresso.Correta = correta;
            progresso.Status = StatusProgresso.Concluido;
            progresso.DataConclusao = DateTime.UtcNow;

            AtualizarOfensiva(usuario!);

            if (correta)
            {
                var valorFinal = AplicarMultiplicador(usuario!, progresso.LicaoFala.ValorGanho);
                progresso.PontuacaoObtida = valorFinal;
                usuario!.SaldoAtual += valorFinal;

                _context.Notificacoes.Add(new Notificacao
                {
                    UsuarioId = UsuarioLogadoId,
                    Descricao = $"Lição concluída! 🗣️ Você acertou a fala e ganhou {valorFinal} moedas."
                });
            }
            else
            {
                progresso.PontuacaoObtida = 0;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                progresso.Correta,
                progresso.PontuacaoObtida,
                ScoreAcustico = avaliacao.ScoreGeral,
                TextoReconhecido = avaliacao.TextoFalado,
                NovoSaldo = usuario!.SaldoAtual
            });
        }

        private async Task<AvaliacaoFalaIaDto> AvaliarAudioComIa(string textoEsperado, IFormFile arquivo)
        {
            var baseUrl = _config["Ia:MmsBaseUrl"] ?? "http://127.0.0.1:8000";

            using var conteudoAudio = new StreamContent(arquivo.OpenReadStream());
            conteudoAudio.Headers.ContentType = new MediaTypeHeaderValue(arquivo.ContentType ?? "audio/wav");

            using var form = new MultipartFormDataContent
            {
                { new StringContent(textoEsperado), "texto_esperado" },
                { conteudoAudio, "arquivo", arquivo.FileName }
            };

            var httpClient = _httpClientFactory.CreateClient();
            var resposta = await httpClient.PostAsync($"{baseUrl}/avaliar-fala-alinhada/", form);

            if (!resposta.IsSuccessStatusCode)
                throw new HttpRequestException($"Serviço de IA retornou {(int)resposta.StatusCode}.");

            var resultado = await resposta.Content.ReadFromJsonAsync<AvaliacaoFalaIaDto>();
            return resultado ?? throw new HttpRequestException("Resposta vazia do serviço de IA.");
        }

        [HttpGet("desempenho/{usuarioId}")]
        public async Task<IActionResult> GetDesempenho(int usuarioId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Usuario" && UsuarioLogadoId != usuarioId) return Forbid();

            if (role == "Profissional")
            {
                var ehPaciente = await _context.Usuarios
                    .AnyAsync(u => u.IdUsuario == usuarioId && u.ProfissionalId == UsuarioLogadoId);
                if (!ehPaciente) return Forbid();
            }

            var concluidasAlternativas = await _context.ProgressosAlternativa
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido);
            var corretasAlternativas = await _context.ProgressosAlternativa
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido && p.Correta);

            var concluidasFala = await _context.ProgressosFala
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido);
            var corretasFala = await _context.ProgressosFala
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido && p.Correta);

            double percentInterpretacao = concluidasAlternativas == 0 ? 0 : (double)corretasAlternativas / concluidasAlternativas * 100;
            double percentFala = concluidasFala == 0 ? 0 : (double)corretasFala / concluidasFala * 100;

            return Ok(new
            {
                Interpretacao = Math.Round(percentInterpretacao, 1),
                Fala = Math.Round(percentFala, 1)
            });
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetProgressoUsuario(int usuarioId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Usuario" && UsuarioLogadoId != usuarioId) return Forbid();
            if (role == "Profissional")
            {
                var ehPaciente = await _context.Usuarios
                    .AnyAsync(u => u.IdUsuario == usuarioId && u.ProfissionalId == UsuarioLogadoId);
                if (!ehPaciente) return Forbid();
            }

            var alternativas = await _context.ProgressosAlternativa
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new { p.IdProgresso, Tipo = "Alternativa", p.LicaoAlternativaId, p.Status, p.PontuacaoObtida, p.DataInicio, p.DataConclusao })
                .ToListAsync();

            var falas = await _context.ProgressosFala
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new { p.IdProgresso, Tipo = "Fala", p.LicaoFalaId, p.Status, p.PontuacaoObtida, p.DataInicio, p.DataConclusao })
                .ToListAsync();

            return Ok(new { Alternativas = alternativas, Falas = falas });
        }

        private void AtualizarOfensiva(Usuario usuario)
        {
            var hoje = DateTime.UtcNow.Date;

            if (usuario.UltimaAtividadeData?.Date == hoje)
                return;

            var ontem = hoje.AddDays(-1);
            bool manteveSequencia = usuario.UltimaAtividadeData?.Date == ontem
                || (usuario.OfensivaCongeladaAte.HasValue && usuario.OfensivaCongeladaAte.Value.Date >= ontem);

            usuario.DiasSeguidos = manteveSequencia ? usuario.DiasSeguidos + 1 : 1;
            usuario.UltimaAtividadeData = hoje;
        }

        private decimal AplicarMultiplicador(Usuario usuario, decimal valorBase)
        {
            if (usuario.MultiplicadorMoedasAte.HasValue
                && usuario.MultiplicadorMoedasAte.Value >= DateTime.UtcNow
                && usuario.MultiplicadorMoedasValor.HasValue)
            {
                return valorBase * usuario.MultiplicadorMoedasValor.Value;
            }

            return valorBase;
        }
    }
}