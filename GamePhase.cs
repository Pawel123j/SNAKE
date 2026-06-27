namespace ElegantSnake
{
    /// <summary>Aktualny stan ekranu gry.</summary>
    public enum GamePhase
    {
        /// <summary>Ekran startowy — czekamy na rozpoczęcie rozgrywki.</summary>
        Ready,

        /// <summary>Trwa rozgrywka.</summary>
        Playing,

        /// <summary>Gra wstrzymana przez gracza.</summary>
        Paused,

        /// <summary>Koniec gry.</summary>
        GameOver
    }
}
