using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    /// <summary>
    /// Verifica e concede conquistas automaticamente ao usuario apos cada atividade concluida.
    /// As conquistas sao idempotentes: nunca concede a mesma conquista duas vezes.
    /// </summary>
    public class ConquistaService
    {
        private readonly AppDbContext _context;

        public ConquistaService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Avalia todas as conquistas pendentes do usuario e concede as que foram atingidas.
        /// Retorna os nomes das conquistas recem-concedidas.
        /// </summary>
        public async Task<List<string>> VerificarEConcederAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return [];

            var idsJaTem = await _context.ConquistasUsuario
                .Where(cu => cu.UsuarioId == usuarioId)
                .Select(cu => cu.ConquistaId)
                .ToListAsync();

            var conquistasPendentes = await _context.Conquistas
                .Where(c => !idsJaTem.Contains(c.IdConquista))
                .ToListAsync();

            if (conquistasPendentes.Count == 0) return [];

            var totalConcluidasAlternativa = await _context.ProgressosAlternativa
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido);

            var totalConcluidasFala = await _context.ProgressosFala
                .CountAsync(p => p.UsuarioId == usuarioId && p.Status == StatusProgresso.Concluido);

            var totalConcluidas = totalConcluidasAlternativa + totalConcluidasFala;

            var conquistasGanhas = new List<string>();

            foreach (var conquista in conquistasPendentes)
            {
                bool atingida = AvaliarRegra(conquista.Nome, usuario, totalConcluidas, totalConcluidasFala);
                if (!atingida) continue;

                _context.ConquistasUsuario.Add(new ConquistaUsuario
                {
                    UsuarioId = usuarioId,
                    ConquistaId = conquista.IdConquista,
                    DataConquista = DateTime.UtcNow
                });

                _context.Notificacoes.Add(new Notificacao
                {
                    UsuarioId = usuarioId,
                    Descricao = $"🏆 Nova conquista desbloqueada! Parabéns, você ganhou: {conquista.Nome}."
                });

                conquistasGanhas.Add(conquista.Nome);
            }

            if (conquistasGanhas.Count > 0)
                await _context.SaveChangesAsync();

            return conquistasGanhas;
        }

        private static bool AvaliarRegra(string nomeConquista, Usuario usuario, int totalConcluidas, int totalConcluidasFala)
        {
            return nomeConquista switch
            {
                "Primeiros Passos" => totalConcluidas >= 1,
                "Semana de Ouro" => usuario.DiasSeguidos >= 7,
                "Voz Firme" => totalConcluidasFala >= 10,
                "Dedicação Total" => usuario.DiasSeguidos >= 30,
                "Colecionador" => false,
                _ => false
            };
        }
    }
}
