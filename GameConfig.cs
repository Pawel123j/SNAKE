namespace ElegantSnake
{
    /// <summary>
    /// Centralne ustawienia rozgrywki i wymiarów planszy.
    /// Zmiana wartości tutaj wpływa na cały projekt.
    /// </summary>
    public static class GameConfig
    {
        /// <summary>Liczba komórek w pionie i poziomie.</summary>
        public const int GridSize = 26;

        /// <summary>Rozmiar pojedynczej komórki w pikselach.</summary>
        public const int CellSize = 24;

        /// <summary>Wysokość górnego paska HUD.</summary>
        public const int TopHudHeight = 78;

        /// <summary>Margines wokół planszy.</summary>
        public const int BorderPadding = 18;

        /// <summary>Początkowy odstęp między krokami węża (ms) — im mniej, tym szybciej.</summary>
        public const int StartIntervalMs = 95;

        /// <summary>Najmniejszy możliwy odstęp między krokami (ms).</summary>
        public const int MinIntervalMs = 55;

        /// <summary>Liczba punktów za zjedzenie jednego owocu.</summary>
        public const int PointsPerFood = 10;

        /// <summary>Szerokość całego obszaru gry w pikselach.</summary>
        public const int BoardPixelWidth = GridSize * CellSize + BorderPadding * 2;

        /// <summary>Wysokość całego obszaru gry w pikselach (plansza + HUD).</summary>
        public const int BoardPixelHeight = GridSize * CellSize + BorderPadding * 2 + TopHudHeight;
    }
}
