using System.Drawing;
using System.Drawing.Drawing2D;

namespace ElegantSnake
{
    /// <summary>Pomocnicze funkcje rysujące współdzielone przez renderer.</summary>
    public static class GraphicsUtils
    {
        /// <summary>Tworzy ścieżkę prostokąta z zaokrąglonymi rogami.</summary>
        public static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        /// <summary>Zwraca prostokąt powiększony równomiernie o podaną wartość.</summary>
        public static Rectangle Inflate(Rectangle rect, int amount)
        {
            return new Rectangle(
                rect.X - amount,
                rect.Y - amount,
                rect.Width + amount * 2,
                rect.Height + amount * 2);
        }
    }
}
