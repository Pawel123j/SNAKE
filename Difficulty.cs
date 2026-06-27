namespace ElegantSnake
{
    /// <summary>Dostępne poziomy trudności.</summary>
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    /// <summary>
    /// Parametry rozgrywki zależne od poziomu trudności: tempo, owijanie
    /// ścian i liczba przeszkód.
    /// </summary>
    public sealed class DifficultySettings
    {
        private DifficultySettings(Difficulty difficulty, string name,
            int startIntervalMs, int minIntervalMs, bool wrapAround, int obstacleCount)
        {
            Difficulty = difficulty;
            Name = name;
            StartIntervalMs = startIntervalMs;
            MinIntervalMs = minIntervalMs;
            WrapAround = wrapAround;
            ObstacleCount = obstacleCount;
        }

        public Difficulty Difficulty { get; }
        public string Name { get; }

        /// <summary>Początkowy odstęp między krokami węża (ms).</summary>
        public int StartIntervalMs { get; }

        /// <summary>Najmniejszy możliwy odstęp między krokami (ms).</summary>
        public int MinIntervalMs { get; }

        /// <summary>Czy wyjście poza krawędź przenosi węża na drugą stronę.</summary>
        public bool WrapAround { get; }

        /// <summary>Liczba przeszkód na planszy.</summary>
        public int ObstacleCount { get; }

        public static DifficultySettings For(Difficulty difficulty) => difficulty switch
        {
            Difficulty.Easy => new DifficultySettings(Difficulty.Easy, "Łatwy", 130, 80, true, 0),
            Difficulty.Hard => new DifficultySettings(Difficulty.Hard, "Trudny", 80, 45, false, 14),
            _ => new DifficultySettings(Difficulty.Normal, "Normalny", 95, 55, false, 0)
        };
    }
}
