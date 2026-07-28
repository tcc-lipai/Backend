using System.Security.Claims;
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
    [Authorize]
    public class ProgressoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProgressoController(AppDbContext context)
        {
            _context = context;
        }

        private int UsuarioLogadoId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ==========================================
        // ALTERNATIVA
        // ==========================================
        [HttpPost("alternativa/{licaoId}/iniciar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> IniciarAlternativa(int licaoId)
        {
            var licao = await _context.LicoesAlternativa.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "Lição não encontrada." });

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

        // ==========================================
        // FALA
        // ==========================================
        [HttpPost("fala/{licaoId}/iniciar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> IniciarFala(int licaoId)
        {
            var licao = await _context.LicoesFala.FindAsync(licaoId);
            if (licao == null) return NotFound(new { message = "Lição não encontrada." });

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

        // Recebe o texto já reconhecido pela IA de voz (isso roda fora do backend .NET —
        // o front manda o áudio pra IA, e manda só o texto transcrito de volta pra cá)
        [HttpPost("fala/{idProgresso}/concluir")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConcluirFala(int idProgresso, ResponderFalaDto dto)
        {
            var progresso = await _context.ProgressosFala
                .Include(p => p.LicaoFala)
                .FirstOrDefaultAsync(p => p.IdProgresso == idProgresso);

            if (progresso == null) return NotFound(new { message = "Progresso não encontrado." });
            if (progresso.UsuarioId != UsuarioLogadoId) return Forbid();
            if (progresso.Status == StatusProgresso.Concluido)
                return BadRequest(new { message = "Essa tentativa já foi concluída." });

            var usuario = await _context.Usuarios.FindAsync(UsuarioLogadoId);

            bool correta = progresso.LicaoFala!.FraseEsperada.Trim()
                .Equals(dto.TextoReconhecidoPelaIA.Trim(), StringComparison.OrdinalIgnoreCase);

            progresso.TextoReconhecidoPelaIA = dto.TextoReconhecidoPelaIA;
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
                NovoSaldo = usuario!.SaldoAtual
            });
        }

        // ==========================================
        // DESEMPENHO (gráficos do dashboard e da tela do profissional)
        // ==========================================
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

            var totalAlternativas = await _context.ProgressosAlternativa.CountAsync(p => p.UsuarioId == usuarioId);
            var concluidasAlternativas = await _context.ProgressosAlternativa
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido);

            var totalFala = await _context.ProgressosFala.CountAsync(p => p.UsuarioId == usuarioId);
            var concluidasFala = await _context.ProgressosFala
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido);

            double percentInterpretacao = totalAlternativas == 0 ? 0 : (double)concluidasAlternativas / totalAlternativas * 100;
            double percentFala = totalFala == 0 ? 0 : (double)concluidasFala / totalFala * 100;

            return Ok(new
            {
                Interpretacao = Math.Round(percentInterpretacao, 1),
                Fala = Math.Round(percentFala, 1)
            });
        }

        // ==========================================
        // LISTA DE PROGRESSO DO USUARIO ("Início Atividades" / "Continuar atividade")
        // ==========================================
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

        // ==========================================
        // HELPERS PRIVADOS — ofensiva e multiplicador de moedas
        // ==========================================
        private void AtualizarOfensiva(Usuario usuario)
        {
            var hoje = DateTime.UtcNow.Date;

            if (usuario.UltimaAtividadeData?.Date == hoje)
                return; // já contou hoje, não mexe de novo

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