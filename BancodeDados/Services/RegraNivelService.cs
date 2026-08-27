using Backend.Models;

namespace Backend.Services
{
    public static class RegraNivelService
    {
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