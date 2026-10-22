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

            var licao = await _context.LicoesAlternativa.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "LiÃ§Ã£o nÃ£o encontrada." });

            if (!RegraNivelService.PodeAcessarConteudo(usuario.NivelDificuldade, licao.NivelDificuldade))
                return Forbid();

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
                bool jaConcluiuAntes = await _context.ProgressosAlternativa.AnyAsync(p =>
                    p.UsuarioId == UsuarioLogadoId
                    && p.LicaoAlternativaId == progresso.LicaoAlternativaId
                    && p.Status == StatusProgresso.Concluido
                    && p.Correta
                    && p.IdProgresso != progresso.IdProgresso);

                if (jaConcluiuAntes)
                {
                    // já passou dessa lição antes — refazer não paga de novo,
                    // só registra a tentativa (evita farmar moeda repetindo).
                    progresso.PontuacaoObtida = 0;
                }
                else
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
            }
            else
            {
                progresso.PontuacaoObtida = 0;
            }

            ConsumirMultiplicador(usuario!);

            await _context.SaveChangesAsync();

            // Verifica conquistas desbloqueadas após conclusão
            var novasConquistas = await _conquistaService.VerificarEConcederAsync(UsuarioLogadoId);

            return Ok(new
            {
                progresso.Correta,
                progresso.PontuacaoObtida,
                NovoSaldo = usuario!.SaldoAtual,
                NovasConquistas = novasConquistas
            });
        }

        [HttpPost("fala/{licaoId}/iniciar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> IniciarFala(int licaoId)
        {

            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            var licao = await _context.LicoesFala.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "LiÃ§Ã£o nÃ£o encontrada." });

            if (!RegraNivelService.PodeAcessarConteudo(usuario.NivelDificuldade, licao.NivelDificuldade))
                return Forbid();

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
        public async Task<IActionResult> ConcluirFala(int idProgresso, IFormFile arquivo, CancellationToken ct)
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
                    progresso.LicaoFala!.FraseEsperada, arquivo, usuario!.EstadoAdaptativoIA, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // o aluno saiu da tela (ou fechou a aba) enquanto a IA avaliava: nada foi
                // gravado ainda, a tentativa continua pendente e o trabalho para aqui.
                return StatusCode(499);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(504, new { message = "A avaliacao do audio demorou demais. Tente novamente." });
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
                bool jaConcluiuAntes = await _context.ProgressosFala.AnyAsync(p =>
                    p.UsuarioId == UsuarioLogadoId
                    && p.LicaoFalaId == progresso.LicaoFalaId
                    && p.Status == StatusProgresso.Concluido
                    && p.Correta
                    && p.IdProgresso != progresso.IdProgresso);

                if (jaConcluiuAntes)
                {
                    // já passou dessa lição antes — refazer não paga de novo,
                    // só registra a tentativa (evita farmar moeda repetindo).
                    progresso.PontuacaoObtida = 0;
                }
                else
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
            }
            else
            {
                progresso.PontuacaoObtida = 0;
            }

            ConsumirMultiplicador(usuario!);

            await _context.SaveChangesAsync();

            var novasConquistas = await _conquistaService.VerificarEConcederAsync(UsuarioLogadoId);

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
                NovoSaldo = usuario!.SaldoAtual,
                NovasConquistas = novasConquistas
            });
        }

        [HttpPost("video/{licaoId}/concluir")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConcluirVideo(int licaoId)
        {
            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            var licao = await _context.LicoesVideo.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "LiÃ§Ã£o nÃ£o encontrada." });

            if (!RegraNivelService.PodeAcessarConteudo(usuario.NivelDificuldade, licao.NivelDificuldade))
                return Forbid();

            var progressoExistente = await _context.ProgressosVideo
                .FirstOrDefaultAsync(p => p.UsuarioId == UsuarioLogadoId && p.LicaoVideoId == licaoId);

            if (progressoExistente != null)
                return Ok(new { progressoExistente.IdProgresso });

            var progresso = new ProgressoVideo
            {
                UsuarioId = UsuarioLogadoId,
                LicaoVideoId = licaoId,
                Status = StatusProgresso.Concluido,
                DataInicio = DateTime.UtcNow,
                DataConclusao = DateTime.UtcNow
            };

            _context.ProgressosVideo.Add(progresso);
            AtualizarOfensiva(usuario);
            await _context.SaveChangesAsync();

            var novasConquistas = await _conquistaService.VerificarEConcederAsync(UsuarioLogadoId);

            return Ok(new { progresso.IdProgresso, novasConquistas });
        }

        [HttpPost("vibracao/{licaoId}/concluir")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConcluirVibracao(int licaoId)
        {
            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            if (!RegraNivelService.PodeAcessarVibracao(usuario.NivelDificuldade))
                return Forbid();

            var licao = await _context.LicoesVibracao.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "Lição não encontrada." });

            var progressoExistente = await _context.ProgressosVibracao
                .FirstOrDefaultAsync(p => p.UsuarioId == UsuarioLogadoId && p.LicaoVibracaoId == licaoId);

            if (progressoExistente != null)
                return Ok(new { progressoExistente.IdProgresso });

            var progresso = new ProgressoVibracao
            {
                UsuarioId = UsuarioLogadoId,
                LicaoVibracaoId = licaoId,
                Status = StatusProgresso.Concluido,
                DataInicio = DateTime.UtcNow,
                DataConclusao = DateTime.UtcNow
            };

            _context.ProgressosVibracao.Add(progresso);
            AtualizarOfensiva(usuario);
            await _context.SaveChangesAsync();

            var novasConquistasVibracao = await _conquistaService.VerificarEConcederAsync(UsuarioLogadoId);

            return Ok(new { progresso.IdProgresso, novasConquistas = novasConquistasVibracao });
        }

        /// <summary>
        /// PrÃ³ximo exercÃ­cio de fala do aluno, gerado pelo motor adaptativo da IA
        /// (fonema/nÃ­vel/repetiÃ§Ã£o espaÃ§ada). O estado adaptativo devolvido Ã©
        /// persistido no usuÃ¡rio.
        /// </summary>
        [HttpGet("fala/proximo-exercicio")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ProximoExercicioFala(CancellationToken ct, [FromQuery] bool prova = false)
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
                    $"/ia/proximo-exercicio?estado_adaptativo={estado}&modo_prova={prova.ToString().ToLower()}", ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return StatusCode(499);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(504, new { message = "O servico de IA demorou demais para responder." });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "O serviÃ§o de IA estÃ¡ indisponÃ­vel." });
            }

            if (resposta.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound(new { message = "Ainda nÃ£o hÃ¡ conteÃºdo de fala para o seu nÃ­vel/fonema atual." });
            if (!resposta.IsSuccessStatusCode)
                return StatusCode(502, new { message = "O serviÃ§o de IA retornou um erro." });

            var exercicio = await resposta.Content.ReadFromJsonAsync<ProximoExercicioIaDto>(cancellationToken: ct);
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

        // A avaliacao acustica roda em CPU no servico de IA: limitar quantas rodam ao mesmo
        // tempo evita encher o servico (e as threads daqui) quando varios alunos gravam juntos.
        // Quem passa do limite espera na fila (cancelavel pelo ct, se o aluno desistir).
        private static readonly SemaphoreSlim _limiteAvaliacoesIa = new(3, 3);

        private async Task<AvaliacaoFalaIaDto> AvaliarAudioComIa(
            string textoEsperado, IFormFile arquivo, string? estadoAdaptativo, CancellationToken ct)
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
            HttpResponseMessage resposta;
            await _limiteAvaliacoesIa.WaitAsync(ct);
            try
            {
                resposta = await httpClient.PostAsync("/ia/avaliar-fala", form, ct);
            }
            finally
            {
                _limiteAvaliacoesIa.Release();
            }

            if (!resposta.IsSuccessStatusCode)
                throw new HttpRequestException($"ServiÃ§o de IA retornou {(int)resposta.StatusCode}.");

            var resultado = await resposta.Content.ReadFromJsonAsync<AvaliacaoFalaIaDto>(cancellationToken: ct);
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
                var ehPaciente = await _context.VinculosProfissional
                    .AnyAsync(v => v.UsuarioId == usuarioId && v.ProfissionalId == UsuarioLogadoId);
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
                var ehPaciente = await _context.VinculosProfissional
                    .AnyAsync(v => v.UsuarioId == usuarioId && v.ProfissionalId == UsuarioLogadoId);
                if (!ehPaciente) return Forbid();
            }

            var alternativas = await _context.ProgressosAlternativa
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new { p.IdProgresso, Tipo = "Alternativa", p.LicaoAlternativaId, p.Status, p.Correta, p.PontuacaoObtida, p.DataInicio, p.DataConclusao })
                .ToListAsync();

            var falas = await _context.ProgressosFala
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new { p.IdProgresso, Tipo = "Fala", p.LicaoFalaId, p.Status, p.Correta, p.ScoreAcustico, p.PontuacaoObtida, p.DataInicio, p.DataConclusao })
                .ToListAsync();

            var videos = await _context.ProgressosVideo
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new { p.IdProgresso, Tipo = "Video", p.LicaoVideoId, p.Status, p.DataInicio, p.DataConclusao })
                .ToListAsync();

            var vibracoes = await _context.ProgressosVibracao
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new { p.IdProgresso, Tipo = "Vibracao", p.LicaoVibracaoId, p.Status, p.DataInicio, p.DataConclusao })
                .ToListAsync();

            return Ok(new { Alternativas = alternativas, Falas = falas, Videos = videos, Vibracoes = vibracoes });
        }

        private const double LimiarMediaNivel = 70.0;

        /// <summary>
        /// Checa se o usuário logado pode subir de nível: completou 100% do
        /// conteúdo do nível atual (sem contar níveis anteriores) com média de
        /// acerto >= 70% entre Fala+Alternativa (Vídeo só precisa estar
        /// concluído, não tem noção de acerto). Pro Iniciante o gate é só
        /// completar toda a Vibração (não tem "acerto" nela).
        /// </summary>
        private async Task<(bool Elegivel, NivelDificuldade? ProximoNivel)> CalcularElegibilidadeNivel(Usuario usuario)
        {
            if (usuario.NivelDificuldade == NivelDificuldade.Avancado)
                return (false, null);

            var proximoNivel = (NivelDificuldade)((int)usuario.NivelDificuldade + 1);

            if (usuario.NivelDificuldade == NivelDificuldade.Iniciante)
            {
                var totalVibracao = await _context.LicoesVibracao.CountAsync();
                if (totalVibracao == 0) return (false, proximoNivel);

                var concluidasVibracao = await _context.ProgressosVibracao
                    .CountAsync(p => p.UsuarioId == usuario.IdUsuario && p.Status == StatusProgresso.Concluido);

                return (concluidasVibracao >= totalVibracao, proximoNivel);
            }

            var nivelAtual = usuario.NivelDificuldade;

            var totalFala = await _context.LicoesFala.CountAsync(l => l.NivelDificuldade == nivelAtual);
            var totalAlternativa = await _context.LicoesAlternativa.CountAsync(l => l.NivelDificuldade == nivelAtual);
            var totalVideo = await _context.LicoesVideo.CountAsync(l => l.NivelDificuldade == nivelAtual);

            if (totalFala + totalAlternativa + totalVideo == 0)
                return (false, proximoNivel);

            var falaPorLicao = await (
                from p in _context.ProgressosFala
                join l in _context.LicoesFala on p.LicaoFalaId equals l.IdLicaoFala
                where p.UsuarioId == usuario.IdUsuario && l.NivelDificuldade == nivelAtual && p.Status == StatusProgresso.Concluido
                group p by p.LicaoFalaId into g
                select g.Any(x => x.Correta)
            ).ToListAsync();

            var alternativaPorLicao = await (
                from p in _context.ProgressosAlternativa
                join l in _context.LicoesAlternativa on p.LicaoAlternativaId equals l.IdLicaoAlternativa
                where p.UsuarioId == usuario.IdUsuario && l.NivelDificuldade == nivelAtual && p.Status == StatusProgresso.Concluido
                group p by p.LicaoAlternativaId into g
                select g.Any(x => x.Correta)
            ).ToListAsync();

            var concluidasVideo = await (
                from p in _context.ProgressosVideo
                join l in _context.LicoesVideo on p.LicaoVideoId equals l.IdLicaoVideo
                where p.UsuarioId == usuario.IdUsuario && l.NivelDificuldade == nivelAtual && p.Status == StatusProgresso.Concluido
                select p.LicaoVideoId
            ).Distinct().CountAsync();

            bool completouTudo = falaPorLicao.Count >= totalFala
                && alternativaPorLicao.Count >= totalAlternativa
                && concluidasVideo >= totalVideo;

            if (!completouTudo) return (false, proximoNivel);

            var avaliados = falaPorLicao.Count + alternativaPorLicao.Count;
            if (avaliados == 0) return (true, proximoNivel); // só tinha vídeo nesse nível

            var corretos = falaPorLicao.Count(c => c) + alternativaPorLicao.Count(c => c);
            double media = (double)corretos / avaliados * 100;

            return (media >= LimiarMediaNivel, proximoNivel);
        }

        [HttpGet("nivel/elegibilidade")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> GetElegibilidadeNivel()
        {
            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            var (elegivel, proximoNivel) = await CalcularElegibilidadeNivel(usuario);

            return Ok(new
            {
                NivelAtual = usuario.NivelDificuldade,
                ProximoNivel = proximoNivel,
                Elegivel = elegivel
            });
        }

        [HttpPost("nivel/subir")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> SubirNivel()
        {
            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);
            if (usuario == null) return NotFound();

            var (elegivel, proximoNivel) = await CalcularElegibilidadeNivel(usuario);
            if (!elegivel || proximoNivel == null)
                return BadRequest(new { message = "Você ainda não completou os requisitos pra subir de nível." });

            usuario.NivelDificuldade = proximoNivel.Value;
            await _context.SaveChangesAsync();

            return Ok(new { NovoNivel = usuario.NivelDificuldade });
        }

        [HttpGet("desempenho-unidades/{usuarioId}")]
        public async Task<IActionResult> GetDesempenhoPorUnidade(int usuarioId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Usuario" && UsuarioLogadoId != usuarioId) return Forbid();
            if (role == "Profissional")
            {
                var ehPaciente = await _context.VinculosProfissional
                    .AnyAsync(v => v.UsuarioId == usuarioId && v.ProfissionalId == UsuarioLogadoId);
                if (!ehPaciente) return Forbid();
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return NotFound();
            var nivelUsuario = usuario.NivelDificuldade;

            var falaPorUnidade = await (
                from p in _context.ProgressosFala
                join l in _context.LicoesFala on p.LicaoFalaId equals l.IdLicaoFala
                where p.UsuarioId == usuarioId && l.NivelDificuldade <= nivelUsuario && p.Status == StatusProgresso.Concluido
                group p by l.UnidadeId into g
                select new { UnidadeId = g.Key, Concluidas = g.Count(), Corretas = g.Count(x => x.Correta) }
            ).ToListAsync();

            var alternativaPorUnidade = await (
                from p in _context.ProgressosAlternativa
                join l in _context.LicoesAlternativa on p.LicaoAlternativaId equals l.IdLicaoAlternativa
                where p.UsuarioId == usuarioId && l.NivelDificuldade <= nivelUsuario && p.Status == StatusProgresso.Concluido
                group p by l.UnidadeId into g
                select new { UnidadeId = g.Key, Concluidas = g.Count(), Corretas = g.Count(x => x.Correta) }
            ).ToListAsync();

            var videoPorUnidade = await (
                from p in _context.ProgressosVideo
                join l in _context.LicoesVideo on p.LicaoVideoId equals l.IdLicaoVideo
                where p.UsuarioId == usuarioId && l.NivelDificuldade <= nivelUsuario && p.Status == StatusProgresso.Concluido
                group p by l.UnidadeId into g
                select new { UnidadeId = g.Key, Concluidos = g.Select(x => x.LicaoVideoId).Distinct().Count() }
            ).ToListAsync();

            var totalVideoPorUnidade = await _context.LicoesVideo
                .Where(l => l.NivelDificuldade <= nivelUsuario)
                .GroupBy(l => l.UnidadeId)
                .Select(g => new { UnidadeId = g.Key, Total = g.Count() })
                .ToListAsync();

            var vibracaoPorUnidade = await (
                from p in _context.ProgressosVibracao
                join l in _context.LicoesVibracao on p.LicaoVibracaoId equals l.IdLicaoVibracao
                where p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido
                group p by l.UnidadeId into g
                select new { UnidadeId = g.Key, Concluidas = g.Select(x => x.LicaoVibracaoId).Distinct().Count() }
            ).ToListAsync();

            var totalVibracaoPorUnidade = await _context.LicoesVibracao
                .GroupBy(l => l.UnidadeId)
                .Select(g => new { UnidadeId = g.Key, Total = g.Count() })
                .ToListAsync();

            var unidades = await _context.Unidades.Select(u => new { u.IdUnidade, u.Nome }).ToListAsync();

            double? Acerto(int? concluidas, int? corretas) =>
                concluidas is null or 0 ? null : Math.Round((double)corretas! / concluidas.Value * 100, 1);

            int? ConcluidoPercentual(int concluidos, int total) =>
                total == 0 ? null : (int)Math.Round((double)concluidos / total * 100);

            var resultado = unidades.Select(u =>
            {
                var fala = falaPorUnidade.FirstOrDefault(x => x.UnidadeId == u.IdUnidade);
                var alternativa = alternativaPorUnidade.FirstOrDefault(x => x.UnidadeId == u.IdUnidade);
                var video = videoPorUnidade.FirstOrDefault(x => x.UnidadeId == u.IdUnidade);
                var totalVideo = totalVideoPorUnidade.FirstOrDefault(x => x.UnidadeId == u.IdUnidade)?.Total ?? 0;
                var vibracao = vibracaoPorUnidade.FirstOrDefault(x => x.UnidadeId == u.IdUnidade);
                var totalVibracao = totalVibracaoPorUnidade.FirstOrDefault(x => x.UnidadeId == u.IdUnidade)?.Total ?? 0;

                var acertoFala = Acerto(fala?.Concluidas, fala?.Corretas);
                var acertoAlternativa = Acerto(alternativa?.Concluidas, alternativa?.Corretas);

                double? acertoGeral = null;
                int concluidasGeral = (fala?.Concluidas ?? 0) + (alternativa?.Concluidas ?? 0);
                int corretasGeral = (fala?.Corretas ?? 0) + (alternativa?.Corretas ?? 0);
                if (concluidasGeral > 0) acertoGeral = Math.Round((double)corretasGeral / concluidasGeral * 100, 1);

                return new
                {
                    UnidadeId = u.IdUnidade,
                    Nome = u.Nome,
                    AcertoFala = acertoFala,
                    AcertoInterpretacao = acertoAlternativa,
                    ConcluidoVideo = ConcluidoPercentual(video?.Concluidos ?? 0, totalVideo),
                    ConcluidoVibracao = ConcluidoPercentual(vibracao?.Concluidas ?? 0, totalVibracao),
                    AcertoGeralUnidade = acertoGeral
                };
            }).ToList();

            return Ok(resultado);
        }

        [HttpGet("ofensiva/{usuarioId}")]
        public async Task<IActionResult> GetHistoricoOfensiva(int usuarioId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Usuario" && UsuarioLogadoId != usuarioId) return Forbid();
            if (role == "Profissional")
            {
                var ehPaciente = await _context.VinculosProfissional
                    .AnyAsync(v => v.UsuarioId == usuarioId && v.ProfissionalId == UsuarioLogadoId);
                if (!ehPaciente) return Forbid();
            }

            var historico = await _context.HistoricosOfensiva
                .Where(h => h.UsuarioId == usuarioId)
                .OrderBy(h => h.Data)
                .Select(h => new { h.Data, h.Status })
                .ToListAsync();

            return Ok(historico);
        }

        private static readonly TimeZoneInfo FusoBrasil = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

        /// <summary>
        /// "Hoje" no fuso do Brasil, não em UTC — perto da virada da meia-noite
        /// UTC (equivalente a ~21h no horário de Brasília) o dia em UTC já
        /// avança enquanto ainda é "ontem" para o usuário, o que quebrava a
        /// contagem de ofensiva e o calendário.
        /// </summary>
        private static DateTime HojeNoBrasil() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoBrasil).Date;

        private void AtualizarOfensiva(Usuario usuario)
        {
            var hoje = HojeNoBrasil();

            if (usuario.UltimaAtividadeData?.Date == hoje)
                return;

            var ontem = hoje.AddDays(-1);
            bool congelouGap = usuario.UltimaAtividadeData.HasValue
                && usuario.UltimaAtividadeData.Value.Date != ontem
                && usuario.OfensivaCongeladaAte.HasValue
                && usuario.OfensivaCongeladaAte.Value.Date >= ontem;
            bool manteveSequencia = usuario.UltimaAtividadeData?.Date == ontem || congelouGap;

            if (congelouGap)
            {
                for (var dia = usuario.UltimaAtividadeData!.Value.Date.AddDays(1); dia <= ontem; dia = dia.AddDays(1))
                {
                    _context.HistoricosOfensiva.Add(new HistoricoOfensiva
                    {
                        UsuarioId = usuario.IdUsuario,
                        Data = dia,
                        Status = "Congelada"
                    });
                }
            }

            usuario.DiasSeguidos = manteveSequencia ? usuario.DiasSeguidos + 1 : 1;
            usuario.UltimaAtividadeData = hoje;

            _context.HistoricosOfensiva.Add(new HistoricoOfensiva
            {
                UsuarioId = usuario.IdUsuario,
                Data = hoje,
                Status = "Realizada"
            });
        }

        private decimal AplicarMultiplicador(Usuario usuario, decimal valorBase)
        {
            if (usuario.MultiplicadorMoedasAtividadesRestantes.HasValue
                && usuario.MultiplicadorMoedasAtividadesRestantes.Value > 0
                && usuario.MultiplicadorMoedasValor.HasValue)
            {
                return valorBase * usuario.MultiplicadorMoedasValor.Value;
            }

            return valorBase;
        }

        /// <summary>
        /// Consome uma atividade da janela do multiplicador de moedas (se ativo),
        /// independente do usuário ter acertado ou não a atividade.
        /// </summary>
        private void ConsumirMultiplicador(Usuario usuario)
        {
            if (usuario.MultiplicadorMoedasAtividadesRestantes.HasValue
                && usuario.MultiplicadorMoedasAtividadesRestantes.Value > 0)
            {
                usuario.MultiplicadorMoedasAtividadesRestantes--;
            }
        }
    }
}

