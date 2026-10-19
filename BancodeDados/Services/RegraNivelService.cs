using Backend.Models;

namespace Backend.Services
{
    public static class RegraNivelService
    {
        /// <summary>Vibração representa o nível 1 (Iniciante) e fica sempre liberada.</summary>
        public static bool PodeAcessarVibracao(NivelDificuldade nivel)
        {
            return true;
        }

        /// <summary>
        /// Checagem binária antiga (só bloqueia Iniciante). Mantida só pro motor
        /// adaptativo de fala (`/fala/proximo-exercicio`), que não tem uma lição
        /// específica pra comparar nível — pra conteúdo estático (Fala/Alternativa/
        /// Vídeo) usar PodeAcessarConteudo.
        /// </summary>
        public static bool PodeAcessarOutrosTipos(NivelDificuldade nivel)
        {
            return nivel != NivelDificuldade.Iniciante;
        }

        /// <summary>
        /// Acesso cumulativo: o usuário só acessa conteúdo (Fala/Alternativa/Vídeo)
        /// com nível menor ou igual ao seu próprio.
        /// </summary>
        public static bool PodeAcessarConteudo(NivelDificuldade nivelUsuario, NivelDificuldade nivelConteudo)
        {
            return (int)nivelUsuario >= (int)nivelConteudo;
        }
    }
}