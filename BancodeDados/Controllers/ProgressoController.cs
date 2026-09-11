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
using System.Text.Json;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgressoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Backend.Services.ConquistaService _conquistaService;

        private const double LimiarAcertoFala = 70.0;

        public ProgressoController(AppDbContext context, IHttpClientFactory httpClientFactory, Backend.Services.ConquistaService conquistaService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _conquistaService = conquistaService;
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
            if (licao == null) return NotFound(new { message = "LiÃ§Ã£o nÃ£o encontrada." });

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

            // Verifica conquistas desbloqueadas após conclusão
            var novasConquistas = await _conquistaService.VerificarEConcederAsync(UsuarioLogadoId);

            return Ok(new { progresso.IdProgresso });
        }

        [HttpPost("alternativa/{idProgresso}/concluir")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConcluirAlternativa(int idProgresso, ResponderAlternativaDto dto)
        {
            var progresso = await _context.ProgressosAlternativa
                .Include(p => p.LicaoAlternativa)
                .FirstOrDefaultAsync(p => p.IdProgresso == idProgresso);

            if (progresso == null) return NotFound(new { message = "Progresso nÃ£o encontrado." });
            if (progresso.UsuarioId != UsuarioLogadoId) return Forbid();
            if (progresso.Status == StatusProgresso.Concluido)
                return BadRequest(new { message = "Essa tentativa jÃ¡ foi concluÃ­da." });

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
                    Descricao = $"LiÃ§Ã£o concluÃ­da! ðŸŽ‰ VocÃª acertou e ganhou {valorFinal} moedas."
                });
            }
            else
            {
                progresso.PontuacaoObtida = 0;
            }

            await _context.SaveChangesAsync();

            // Verifica conquistas desbloqueadas após conclusão
            var novasConquistas = await _conquistaService.VerificarEConcederAsync(UsuarioLogadoId);

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
            if (licao == null) return NotFound(new { message = "LiÃ§Ã£o nÃ£o encontrada." });

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

            if (progresso == null) return NotFound(new { message = "Progresso nÃ£o encontrado." });
            if (progresso.UsuarioId != UsuarioLogadoId) return Forbid();
            if (progresso.Status == StatusProgresso.Concluido)
                return BadRequest(new { message = "Essa tentativa jÃ¡ foi concluÃ­da." });

            if (arquivo == null || arquivo.Length == 0)
                return BadRequest(new { message = "Nenhum arquivo de Ã¡udio enviado." });

            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);

            AvaliacaoFalaIaDto avaliacao;
            try
            {
                avaliacao = await AvaliarAudioComIa(
                    progresso.LicaoFala!.FraseEsperada, arquivo, usuario!.EstadoAdaptativoIA);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "NÃ£o foi possÃ­vel avaliar o Ã¡udio â€” o serviÃ§o de IA estÃ¡ indisponÃ­vel." });
            }

            bool correta = avaliacao.ScoreGeral >= LimiarAcertoFala;

            progresso.TextoReconhecidoPelaIA = avaliacao.TextoFalado;
            progresso.ScoreAcustico = avaliacao.ScoreGeral;
            progresso.FeedbackIA = avaliacao.FeedbackClinico;
            progresso.DetalhesFonemasJson = JsonSerializer.Serialize(avaliacao.DetalhesFonemas);
            progresso.Correta = correta;
            progresso.Status = StatusProgresso.Concluido;
            progresso.DataConclusao = DateTime.UtcNow;

            if (avaliacao.EstadoAdaptativo.ValueKind == JsonValueKind.Object)
                usuario!.EstadoAdaptativoIA = avaliacao.EstadoAdaptativo.GetRawText();

            AtualizarOfensiva(usuario!);

            if (correta)
            {
                var valorFinal = AplicarMultiplicador(usuario!, progresso.LicaoFala.ValorGanho);
                progresso.PontuacaoObtida = valorFinal;
                usuario!.SaldoAtual += valorFinal;

                _context.Notificacoes.Add(new Notificacao
                {
                    UsuarioId = UsuarioLogadoId,
                    Descricao = $"LiÃ§Ã£o concluÃ­da! ðŸ—£ï¸ VocÃª acertou a fala e ganhou {valorFinal} moedas."
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
                Feedback = avaliacao.FeedbackClinico,
                FeedbackOrigem = avaliacao.FeedbackOrigem,
                DetalhesFonemas = avaliacao.DetalhesFonemas,
                DecisaoAdaptativa = avaliacao.DecisaoAdaptativa,
                NovoSaldo = usuario!.SaldoAtual
            });
        }

        /// <summary>
        /// PrÃ³ximo exercÃ­cio de fala do aluno, gerado pelo motor adaptativo da IA
        /// (fonema/nÃ­vel/repetiÃ§Ã£o espaÃ§ada). O estado adaptativo devolvido Ã©
        /// persistido no usuÃ¡rio.
        /// </summary>
        [HttpGet("fala/proximo-exercicio")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ProximoExercicioFala([FromQuery] bool prova = false)
        {
            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            if (!RegraNivelService.PodeAcessarOutrosTipos(usuario.NivelDificuldade))
                return Forbid();

            var httpClient = _httpClientFactory.CreateClient("IA");
            var estado = Uri.EscapeDataString(usuario.EstadoAdaptativoIA ?? "");

            HttpResponseMessage resposta;
            try
            {
                resposta = await httpClient.GetAsync(
                    $"/ia/proximo-exercicio?estado_adaptativo={estado}&modo_prova={prova.ToString().ToLower()}");
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "O serviÃ§o de IA estÃ¡ indisponÃ­vel." });
            }

            if (resposta.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound(new { message = "Ainda nÃ£o hÃ¡ conteÃºdo de fala para o seu nÃ­vel/fonema atual." });
            if (!resposta.IsSuccessStatusCode)
                return StatusCode(502, new { message = "O serviÃ§o de IA retornou um erro." });

            var exercicio = await resposta.Content.ReadFromJsonAsync<ProximoExercicioIaDto>();
            if (exercicio is null)
                return StatusCode(502, new { message = "Resposta vazia do serviÃ§o de IA." });

            if (exercicio.EstadoAdaptativo.ValueKind == JsonValueKind.Object)
            {
                usuario.EstadoAdaptativoIA = exercicio.EstadoAdaptativo.GetRawText();
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                exercicio.Fonema,
                exercicio.Nivel,
                exercicio.PassoLicao,
                exercicio.TotalPassos,
                exercicio.ModoProva,
                exercicio.TextoGerado,
                exercicio.Instrucao,
                exercicio.Dica
            });
        }

        private async Task<AvaliacaoFalaIaDto> AvaliarAudioComIa(
            string textoEsperado, IFormFile arquivo, string? estadoAdaptativo)
        {
            using var conteudoAudio = new StreamContent(arquivo.OpenReadStream());
            // O navegador manda algo como "audio/webm;codecs=opus" â€” Parse aceita os
            // parÃ¢metros; o construtor de MediaTypeHeaderValue nÃ£o.
            conteudoAudio.Headers.ContentType =
                MediaTypeHeaderValue.TryParse(arquivo.ContentType, out var tipoConteudo)
                    ? tipoConteudo
                    : new MediaTypeHeaderValue("audio/wav");

            using var form = new MultipartFormDataContent
            {
                { new StringContent(textoEsperado), "texto_esperado" },
                { new StringContent(estadoAdaptativo ?? ""), "estado_adaptativo" },
                { new StringContent("false"), "modo_prova" },
                { conteudoAudio, "arquivo", arquivo.FileName }
            };

            var httpClient = _httpClientFactory.CreateClient("IA");
            var resposta = await httpClient.PostAsync("/ia/avaliar-fala", form);

            if (!resposta.IsSuccessStatusCode)
                throw new HttpRequestException($"ServiÃ§o de IA retornou {(int)resposta.StatusCode}.");

            var resultado = await resposta.Content.ReadFromJsonAsync<AvaliacaoFalaIaDto>();
            if (resultado is null || !resultado.Sucesso)
                throw new HttpRequestException("Resposta invÃ¡lida do serviÃ§o de IA.");
            return resultado;
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

