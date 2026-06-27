namespace ElegantSnake
{
    /// <summary>
    /// Centralne ustawienia wymiarów planszy. Parametry zależne od poziomu
    /// trudności (tempo, owijanie, przeszkody) znajdują się w
    /// <see cref="DifficultySettings"/>.
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

        /// <summary>Szerokość całego obszaru gry w pikselach.</summary>
        public const int BoardPixelWidth = GridSize * CellSize + BorderPadding * 2;

        /// <summary>Wysokość całego obszaru gry w pikselach (plansza + HUD).</summary>
        public const int BoardPixelHeight = GridSize * CellSize + BorderPadding * 2 + TopHudHeight;
    }
}
