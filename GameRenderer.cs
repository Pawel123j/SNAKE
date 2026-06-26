using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace ElegantSnake
{
    /// <summary>Dane potrzebne rendererowi do narysowania jednej klatki.</summary>
    public readonly struct RenderContext
    {
        public RenderContext(int bestScore, double foodPulse, double backgroundShift, GamePhase phase)
        {
            BestScore = bestScore;
            FoodPulse = foodPulse;
            BackgroundShift = backgroundShift;
            Phase = phase;
        }

        public int BestScore { get; }
        public double FoodPulse { get; }
        public double BackgroundShift { get; }
        public GamePhase Phase { get; }
    }

    /// <summary>
    /// Całe rysowanie gry przez <see cref="DrawingContext"/> Avalonii —
    /// tło, HUD, plansza, wąż, jedzenie oraz nakładki (start, pauza, koniec gry).
    /// </summary>
    public sealed class GameRenderer
    {
        private enum GradientDir { Horizontal, Vertical, Diagonal }

        /// <summary>Rysuje całą klatkę gry.</summary>
        public void Draw(DrawingContext c, Size size, SnakeGame game, RenderContext ctx)
        {
            DrawBackground(c, size, ctx.BackgroundShift);
            DrawHud(c, size, game.Score, ctx.BestScore);
            DrawBoard(c);
            DrawFood(c, game.Food, ctx.FoodPulse);
            DrawSnake(c, game);

            switch (ctx.Phase)
            {
                case GamePhase.Ready:
                    DrawCenteredCard(c, "SNAKE", "ENTER lub strzałka — start",
                        "Sterowanie: WASD / strzałki   •   P = pauza");
                    break;
                case GamePhase.Paused:
                    DrawCenteredCard(c, "Pauza", "P / ESC — wznów grę", null);
                    break;
                case GamePhase.GameOver:
                    DrawGameOver(c, game.Score);
                    break;
            }
        }

        private void DrawBackground(DrawingContext c, Size size, double backgroundShift)
        {
            var rect = new Rect(0, 0, size.Width, size.Height);
            c.DrawRectangle(
                Linear(Rgb(15, 18, 30), Rgb(28, 38, 58), GradientDir.Diagonal),
                null, rect);

            // Lekko świecące "halo" w rogach — drobny szczegół tła.
            c.DrawEllipse(Solid(30, 120, 180, 255), null, new Point(150, 10), 190, 110);
            c.DrawEllipse(Solid(26, 90, 255, 210), null,
                new Point(size.Width - 120, size.Height - 100), 160, 120);
        }

        private void DrawHud(DrawingContext c, Size size, int score, int bestScore)
        {
            var hud = new Rect(14, 12, size.Width - 28, 52);

            // Miękki cień pod paskiem.
            c.DrawRectangle(Solid(35, 0, 0, 0), null,
                new Rect(hud.X, hud.Y + 3, hud.Width, hud.Height), 20, 20);

            c.DrawRectangle(
                Linear(Rgb(40, 48, 74), Rgb(24, 28, 42), GradientDir.Horizontal),
                new Pen(Solid(90, 170, 220, 255), 1.2), hud, 20, 20);

            IBrush label = Solid(255, 180, 195, 225);
            IBrush value = Solid(255, 110, 255, 210);

            c.DrawText(Text("SNAKE", Pt(16), Solid(255, 240, 245, 255), FontWeight.Bold), new Point(28, 20));
            c.DrawText(Text("Score", Pt(9), label), new Point(190, 22));
            c.DrawText(Text(score.ToString(), Pt(12), value, FontWeight.Bold), new Point(190, 37));
            c.DrawText(Text("Best", Pt(9), label), new Point(270, 22));
            c.DrawText(Text(bestScore.ToString(), Pt(12), value, FontWeight.Bold), new Point(270, 37));
            c.DrawText(Text("WASD / strzałki   •   P = pauza   •   R = restart", Pt(8.5), label),
                new Point(350, 31));
        }

        private void DrawBoard(DrawingContext c)
        {
            Rect board = BoardGeometry.BoardRect;

            c.DrawRectangle(Solid(50, 0, 0, 0), null,
                new Rect(board.X, board.Y + 5, board.Width, board.Height), 26, 26);

            c.DrawRectangle(
                Linear(Rgb(14, 16, 26), Rgb(18, 24, 36), GradientDir.Vertical),
                new Pen(Solid(80, 165, 195, 255), 1.4), board, 26, 26);

            var gridPen = new Pen(Solid(18, 220, 235, 255), 1);

            for (int x = 1; x < GameConfig.GridSize; x++)
            {
                double px = board.X + x * GameConfig.CellSize;
                c.DrawLine(gridPen, new Point(px, board.Y + 10), new Point(px, board.Bottom - 10));
            }

            for (int y = 1; y < GameConfig.GridSize; y++)
            {
                double py = board.Y + y * GameConfig.CellSize;
                c.DrawLine(gridPen, new Point(board.X + 10, py), new Point(board.Right - 10, py));
            }
        }

        private void DrawSnake(DrawingContext c, SnakeGame game)
        {
            var snake = game.Snake;
            for (int i = snake.Count - 1; i >= 0; i--)
            {
                Rect cell = BoardGeometry.CellToRect(snake[i], 4);
                byte alpha = (byte)Math.Min(70 + i * 8, 185);

                Color start;
                Color end;
                double radius;

                if (i == 0)
                {
                    start = Color.FromArgb(255, 125, 255, 220);
                    end = Color.FromArgb(255, 45, 210, 160);
                    radius = 12;

                    // Poświata wokół głowy.
                    c.DrawRectangle(Solid(55, 100, 255, 220), null, Inflate(cell, 6), 16, 16);
                }
                else
                {
                    start = Color.FromArgb(alpha, 90, 220, 185);
                    end = Color.FromArgb(alpha, 35, 150, 125);
                    radius = 10;
                }

                c.DrawRectangle(
                    Linear(start, end, GradientDir.Diagonal),
                    new Pen(Solid(90, 230, 255, 245), 1), cell, radius, radius);

                if (i == 0)
                    DrawSnakeFace(c, cell, game.CurrentDirection);
            }
        }

        private void DrawSnakeFace(DrawingContext c, Rect head, Direction direction)
        {
            IBrush eye = Solid(230, 10, 24, 28);

            if (direction == Direction.Up || direction == Direction.Down)
            {
                double eyeY = head.Y + 7;
                Dot(c, eye, head.X + 7, eyeY, 4);
                Dot(c, eye, head.Right - 11, eyeY, 4);
            }
            else
            {
                double eyeX = direction == Direction.Right ? head.Right - 10 : head.X + 6;
                Dot(c, eye, eyeX, head.Y + 7, 4);
                Dot(c, eye, eyeX, head.Bottom - 11, 4);
            }
        }

        private void DrawFood(DrawingContext c, PixelPoint food, double foodPulse)
        {
            Rect foodRect = BoardGeometry.CellToRect(food, 5);
            double pulse = Math.Sin(foodPulse) * 3.5;

            c.DrawRectangle(Solid(65, 255, 100, 140), null, Inflate(foodRect, 8 + pulse), 16, 16);

            c.DrawRectangle(
                Linear(Color.FromArgb(255, 255, 120, 155), Color.FromArgb(255, 255, 65, 90), GradientDir.Diagonal),
                new Pen(Solid(170, 255, 230, 235), 1.2), foodRect, 12, 12);

            // Drobny błysk na owocu.
            Dot(c, Solid(140, 255, 255, 255), foodRect.X + 5, foodRect.Y + 4, 6);
        }

        private void DrawGameOver(DrawingContext c, int score)
        {
            Rect board = BoardGeometry.BoardRect;
            var overlay = new Rect(board.X + 65, board.Y + 150, board.Width - 130, 180);

            c.DrawRectangle(Solid(210, 12, 16, 24), new Pen(Solid(120, 255, 120, 150), 1.5), overlay, 28, 28);

            IBrush title = Solid(255, 245, 245, 250);
            IBrush text = Solid(200, 205, 215, 230);
            IBrush accent = Solid(255, 110, 255, 210);
            double cx = overlay.X + overlay.Width / 2;

            DrawCentered(c, Text("Game Over", Pt(24), title, FontWeight.Bold), cx, overlay.Y + 24);
            DrawCentered(c, Text($"Wynik: {score}", Pt(14), accent, FontWeight.Bold), cx, overlay.Y + 82);
            DrawCentered(c, Text("ENTER albo SPACJA — nowa gra", Pt(12), text), cx, overlay.Y + 118);
            DrawCentered(c, Text("R — szybki restart", Pt(12), text), cx, overlay.Y + 145);
        }

        /// <summary>Uniwersalna nakładka z tytułem, podtytułem i opcjonalną podpowiedzią.</summary>
        private void DrawCenteredCard(DrawingContext c, string title, string subtitle, string? hint)
        {
            Rect board = BoardGeometry.BoardRect;
            double height = hint == null ? 150 : 185;
            var overlay = new Rect(
                board.X + 55,
                board.Y + (board.Height - height) / 2,
                board.Width - 110,
                height);

            c.DrawRectangle(Solid(210, 12, 16, 24), new Pen(Solid(120, 110, 255, 210), 1.5), overlay, 28, 28);

            IBrush titleBrush = Solid(255, 245, 245, 250);
            IBrush accent = Solid(255, 110, 255, 210);
            IBrush hintBrush = Solid(200, 205, 215, 230);
            double cx = overlay.X + overlay.Width / 2;

            DrawCentered(c, Text(title, Pt(24), titleBrush, FontWeight.Bold), cx, overlay.Y + 26);
            DrawCentered(c, Text(subtitle, Pt(13), accent, FontWeight.Bold), cx, overlay.Y + 84);
            if (hint != null)
                DrawCentered(c, Text(hint, Pt(11), hintBrush), cx, overlay.Y + 122);
        }

        // ----- pomocnicze -----

        /// <summary>Zamienia rozmiar w punktach (jak w WinForms) na piksele Avalonii (96 DPI).</summary>
        private static double Pt(double points) => points * 96.0 / 72.0;

        private static Color Rgb(byte r, byte g, byte b) => Color.FromArgb(255, r, g, b);

        private static IBrush Solid(byte a, byte r, byte g, byte b) => new SolidColorBrush(Color.FromArgb(a, r, g, b));

        private static IBrush Linear(Color from, Color to, GradientDir dir)
        {
            var (start, end) = dir switch
            {
                GradientDir.Horizontal => (RelativePoint.TopLeft, new RelativePoint(1, 0, RelativeUnit.Relative)),
                GradientDir.Vertical => (RelativePoint.TopLeft, new RelativePoint(0, 1, RelativeUnit.Relative)),
                _ => (RelativePoint.TopLeft, RelativePoint.BottomRight)
            };

            return new LinearGradientBrush
            {
                StartPoint = start,
                EndPoint = end,
                GradientStops =
                {
                    new GradientStop(from, 0),
                    new GradientStop(to, 1)
                }
            };
        }

        private static FormattedText Text(string s, double size, IBrush brush, FontWeight weight = FontWeight.Normal)
            => new FormattedText(
                s,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI", FontStyle.Normal, weight),
                size,
                brush);

        private static void DrawCentered(DrawingContext c, FormattedText ft, double centerX, double y)
            => c.DrawText(ft, new Point(centerX - ft.Width / 2, y));

        private static void Dot(DrawingContext c, IBrush brush, double x, double y, double diameter)
            => c.DrawEllipse(brush, null, new Point(x + diameter / 2, y + diameter / 2), diameter / 2, diameter / 2);

        private static Rect Inflate(Rect r, double amount)
            => new Rect(r.X - amount, r.Y - amount, r.Width + amount * 2, r.Height + amount * 2);
    }
}
