using System.Drawing;

namespace ElegantSnake
{
    /// <summary>Przeliczenia między współrzędnymi siatki a pikselami planszy.</summary>
    public static class BoardGeometry
    {
        /// <summary>Prostokąt całej planszy w pikselach.</summary>
        public static Rectangle BoardRect => new Rectangle(
            GameConfig.BorderPadding,
            GameConfig.TopHudHeight,
            GameConfig.GridSize * GameConfig.CellSize,
            GameConfig.GridSize * GameConfig.CellSize);

        /// <summary>
        /// Zamienia komórkę siatki na prostokąt w pikselach, opcjonalnie
        /// zmniejszony o margines <paramref name="inset"/> z każdej strony.
        /// </summary>
        public static Rectangle CellToRect(Point cell, int inset)
        {
            Rectangle board = BoardRect;
            return new Rectangle(
                board.X + cell.X * GameConfig.CellSize + inset,
                board.Y + cell.Y * GameConfig.CellSize + inset,
                GameConfig.CellSize - inset * 2,
                GameConfig.CellSize - inset * 2);
        }
    }
}
