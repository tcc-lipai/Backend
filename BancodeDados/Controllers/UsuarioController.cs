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
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> PostUsuario(UsuarioCadastroDto dto)
        {
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower())
                || await _context.Profissionais.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower());

            if (emailExiste)
                return BadRequest(new { message = "Este e-mail já está em uso." });

            var (profissionais, erroCodigo) = await ResolverCodigos(dto.CodigoProfissional);
            if (erroCodigo != null) return BadRequest(new { message = erroCodigo });

            var novoUsuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = PasswordHasher.Hash(dto.Senha),
                NivelDificuldade = dto.NivelDificuldade,
                Diagnostico = dto.Diagnostico,
                SaldoAtual = 0,
                Vinculos = profissionais.Select(p => new VinculoProfissional { ProfissionalId = p.IdProfissional }).ToList()
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuarioPorId), new { id = novoUsuario.IdUsuario }, new
            {
                novoUsuario.IdUsuario,
                novoUsuario.Nome,
                novoUsuario.Email,
                novoUsuario.NivelDificuldade,
                VinculadoAProfissional = profissionais.Count > 0
            });
        }

        [HttpPost("vincular-profissional")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> VincularProfissional(VincularProfissionalDto dto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return NotFound();

            var codigo = (dto.CodigoProfissional ?? "").Trim().ToUpperInvariant();
            var profissional = await _context.Profissionais
                .FirstOrDefaultAsync(p => p.CodigoAcesso == codigo);

            if (profissional == null)
                return BadRequest(new { message = "Código de profissional inválido." });

            var vinculos = await _context.VinculosProfissional.Where(v => v.UsuarioId == usuarioId).ToListAsync();
            if (vinculos.Any(v => v.ProfissionalId == profissional.IdProfissional))
                return BadRequest(new { message = "Você já está vinculado a este profissional." });
            if (vinculos.Count >= VinculoProfissional.Maximo)
                return BadRequest(new { message = $"Você já tem {VinculoProfissional.Maximo} profissionais vinculados. Remova um para adicionar outro." });

            _context.VinculosProfissional.Add(new VinculoProfissional
            {
                UsuarioId = usuarioId,
                ProfissionalId = profissional.IdProfissional
            });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conta vinculada com sucesso.", profissional = profissional.Nome });
        }

        /// <summary>Profissionais vinculados ao aluno logado (maximo 3).</summary>
        [HttpGet("meus-profissionais")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> GetMeusProfissionais()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lista = await _context.VinculosProfissional
                .Where(v => v.UsuarioId == usuarioId)
                .OrderBy(v => v.DataVinculo)
                .Select(v => new
                {
                    v.Profissional!.IdProfissional,
                    v.Profissional.Nome,
                    v.Profissional.Email,
                    v.Profissional.Registro,
                    v.DataVinculo
                })
                .ToListAsync();

            return Ok(new { Maximo = VinculoProfissional.Maximo, Profissionais = lista });
        }

        [HttpDelete("meus-profissionais/{profissionalId}")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> DesvincularProfissional(int profissionalId)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var vinculo = await _context.VinculosProfissional
                .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId && v.ProfissionalId == profissionalId);
            if (vinculo == null)
                return NotFound(new { message = "Você não está vinculado a este profissional." });

            _context.VinculosProfissional.Remove(vinculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Resolve uma lista de codigos de profissional (separados por virgula/espaco/;) em profissionais.
        /// Aceita no maximo VinculoProfissional.Maximo codigos distintos.
        /// </summary>
        private async Task<(List<Profissional> Profissionais, string? Erro)> ResolverCodigos(string? codigos)
        {
            var lista = (codigos ?? "")
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(c => c.ToUpperInvariant())
                .Distinct()
                .ToList();

            if (lista.Count == 0) return (new List<Profissional>(), null);
            if (lista.Count > VinculoProfissional.Maximo)
                return (new List<Profissional>(), $"Um aluno pode ter no máximo {VinculoProfissional.Maximo} profissionais vinculados.");

            var encontrados = await _context.Profissionais.Where(p => lista.Contains(p.CodigoAcesso)).ToListAsync();
            if (encontrados.Count != lista.Count)
                return (new List<Profissional>(), "Código de profissional inválido.");

            return (encontrados, null);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUsuario(int id, UsuarioEdicaoAdminDto dto)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Profissional") return Forbid();
            if (role == "Usuario" && idLogado != id) return Forbid();

            var usuarioBanco = await _context.Usuarios.FindAsync(id);
            if (usuarioBanco == null)
                return NotFound(new { message = "Usuário não encontrado." });

            if (usuarioBanco.Email.ToLower() != dto.Email.ToLower())
            {
                var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IdUsuario != id)
                    || await _context.Profissionais.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower());

                if (emailExiste)
                    return BadRequest(new { message = "Este e-mail já está em uso." });
            }

            usuarioBanco.Nome = dto.Nome;
            usuarioBanco.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Senha))
                usuarioBanco.SenhaHash = PasswordHasher.Hash(dto.Senha);
            usuarioBanco.Diagnostico = dto.Diagnostico;
            usuarioBanco.NivelDificuldade = dto.NivelDificuldade;

            if (role == "Admin" && dto.CodigoProfissional != null)
            {
                // o campo traz TODOS os codigos desejados (ate 3, separados por virgula);
                // vazio desvincula todos. Quem nao vier na lista e removido.
                var (desejados, erroCodigo) = await ResolverCodigos(dto.CodigoProfissional);
                if (erroCodigo != null) return BadRequest(new { message = erroCodigo });

                var atuais = await _context.VinculosProfissional.Where(v => v.UsuarioId == id).ToListAsync();
                var idsDesejados = desejados.Select(p => p.IdProfissional).ToHashSet();

                _context.VinculosProfissional.RemoveRange(atuais.Where(v => !idsDesejados.Contains(v.ProfissionalId)));
                foreach (var p in desejados.Where(p => !atuais.Any(v => v.ProfissionalId == p.IdProfissional)))
                    _context.VinculosProfissional.Add(new VinculoProfissional { UsuarioId = id, ProfissionalId = p.IdProfissional });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Notificacoes)
                .Include(u => u.ConquistasUsuario)
                    .ThenInclude(cu => cu.Conquista)
                .Include(u => u.ProdutoUsuarios)
                    .ThenInclude(pu => pu.Produto)
                .Include(u => u.Vinculos)
                .ToListAsync();

            var resultado = usuarios.Select(u => new
            {
                u.IdUsuario,
                u.SaldoAtual,
                u.NivelDificuldade,
                u.Nome,
                u.Email,
                u.Diagnostico,
                ProfissionaisIds = u.Vinculos.Select(v => v.ProfissionalId),
                u.DiasSeguidos,
                u.UltimaAtividadeData,
                OfensivaCongelada = u.OfensivaCongeladaAte > DateTime.UtcNow,
                MultiplicadorAtivo = (u.MultiplicadorMoedasAtividadesRestantes ?? 0) > 0,
                Notificacoes = u.Notificacoes.Select(n => new
                {
                    n.IdNotificacao,
                    n.Descricao,
                    n.Lida,
                    n.DataCriacao
                }),
                Conquistas = u.ConquistasUsuario.Select(cu => new
                {
                    cu.ConquistaId,
                    NomeConquista = cu.Conquista?.Nome,
                    cu.Conquista?.Descricao,
                    cu.Conquista?.IconeUrl,
                    cu.DataConquista
                }),
                ProdutosAdquiridos = u.ProdutoUsuarios.Select(pu => new
                {
                    pu.Id,
                    pu.ProdutoId,
                    NomeProduto = pu.Produto?.Nome,
                    PrecoUnitario = pu.Produto?.Preco,
                    pu.Qtde,
                    pu.TotalCompra,
                    pu.DataCompra
                })
            });

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUsuarioPorId(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var usuario = await _context.Usuarios
                .Include(u => u.Notificacoes)
                .Include(u => u.ConquistasUsuario)
                    .ThenInclude(cu => cu.Conquista)
                .Include(u => u.ProdutoUsuarios)
                    .ThenInclude(pu => pu.Produto)
                .Include(u => u.Vinculos).ThenInclude(v => v.Profissional)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
                return NotFound(new { message = "Usuário não encontrado." });

            if (role == "Usuario" && idLogado != id)
                return Forbid();

            if (role == "Profissional" && !usuario.Vinculos.Any(v => v.ProfissionalId == idLogado))
                return Forbid();

            return Ok(new
            {
                usuario.IdUsuario,
                usuario.SaldoAtual,
                usuario.NivelDificuldade,
                usuario.Nome,
                usuario.Email,
                usuario.Diagnostico,
                ProfissionaisIds = usuario.Vinculos.Select(v => v.ProfissionalId),
                // codigos so pro admin (preenche o formulario de edicao)
                CodigosProfissionais = role == "Admin"
                    ? usuario.Vinculos.Select(v => v.Profissional!.CodigoAcesso).ToList()
                    : null,
                usuario.DiasSeguidos,
                usuario.UltimaAtividadeData,
                OfensivaCongelada = usuario.OfensivaCongeladaAte > DateTime.UtcNow,
                OfensivaCongeladaAte = usuario.OfensivaCongeladaAte,
                MultiplicadorAtivo = (usuario.MultiplicadorMoedasAtividadesRestantes ?? 0) > 0,
                MultiplicadorMoedasAtividadesRestantes = usuario.MultiplicadorMoedasAtividadesRestantes,
                MultiplicadorMoedasValor = usuario.MultiplicadorMoedasValor,
                Notificacoes = usuario.Notificacoes.Select(n => new
                {
                    n.IdNotificacao,
                    n.Descricao,
                    n.Lida,
                    n.DataCriacao
                }),
                Conquistas = usuario.ConquistasUsuario.Select(cu => new
                {
                    cu.ConquistaId,
                    NomeConquista = cu.Conquista?.Nome,
                    cu.Conquista?.Descricao,
                    cu.Conquista?.IconeUrl,
                    cu.DataConquista
                }),
                ProdutosAdquiridos = usuario.ProdutoUsuarios.Select(pu => new
                {
                    pu.Id,
                    pu.ProdutoId,
                    NomeProduto = pu.Produto?.Nome,
                    PrecoUnitario = pu.Produto?.Preco,
                    pu.Qtde,
                    pu.TotalCompra,
                    pu.DataCompra
                })
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}