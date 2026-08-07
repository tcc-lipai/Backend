using Backend.Models;

namespace Backend.Services
{
    public static class RegraNivelService
    {
        // Nível 1 (Iniciante): só Vibração.
        // Nível 2 (Básico): tudo.
        // Nível 3 e 4 (Intermediário/Avançado): tudo, exceto Vibração.
        public static bool PodeAcessarVibracao(NivelDificuldade nivel)
        {
            return nivel == NivelDificuldade.Iniciante || nivel == NivelDificuldade.Basico;
        }

        public static bool PodeAcessarOutrosTipos(NivelDificuldade nivel)
        {
            return nivel != NivelDificuldade.Iniciante;
        }
    }
}