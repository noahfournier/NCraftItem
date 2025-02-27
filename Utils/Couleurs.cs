using Life.Network;

namespace NCraft.Utils
{
    public static class Couleurs
    {
        private static string Format(string couleur, string message)
        {
            return $"<color={couleur}>{message}</color>";
        }

        public static string Bleu(string message) => Format(LifeServer.COLOR_BLUE, message);
        public static string Rouge(string message) => Format(LifeServer.COLOR_RED, message);
        public static string Vert(string message) => Format(LifeServer.COLOR_GREEN, message);
        public static string Orange(string message) => Format(LifeServer.COLOR_ORANGE, message);
        public static string Rose(string message) => Format(LifeServer.COLOR_ME, message);
    }
}
