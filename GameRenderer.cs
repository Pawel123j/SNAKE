using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace ElegantSnake
{
    /// <summary>Dane potrzebne rendererowi do narysowania jednej klatki.</summary>
    public readonly struct RenderContext
    {
        public RenderContext(IReadOnlyList<int> scores, double foodPulse,
            double backgroundShift, GamePhase phase, Difficulty difficulty)
        {
            Scores = scores;
            FoodPulse = foodPulse;
            BackgroundShift = backgroundShift;
            Phase = phase;
            Difficulty = difficulty;
        }

        public IReadOnlyList<int> Scores { get; }
        public double FoodPulse { get; }
        public double BackgroundShift { get; }
        public GamePhase Phase { get; }
        public Difficulty Difficulty { get; }
    }

    /// <summary>
    /// Całe rysowanie gry przez <see cref="DrawingContext"/> Avalonii —
    /// tło, HUD, plansza, przeszkody, wąż, jedzenie oraz nakładki ekranów.
    /// </summary>
    public sealed class GameRenderer
    {
        private enum GradientDir { Horizontal, Vertical, Diagonal }

        /// <summary>Rysuje całą klatkę gry.</summary>
        public void Draw(DrawingContext c, Size size, SnakeGame game, RenderContext ctx)
        {
            int best = ctx.Scores.Count > 0 ? ctx.Scores[0] : 0;

            DrawBackground(c, size, ctx.BackgroundShift);
            DrawHud(c, size, game.Score, best, game.Settings.Name);
            DrawBoard(c);
            DrawObstacles(c, game);
            DrawFood(c, game.Food, game.FoodKind, ctx.FoodPulse);
            DrawSnake(c, game);

            switch (ctx.Phase)
            {
                case GamePhase.Ready:
                    DrawReady(c, ctx.Difficulty, ctx.Scores);
                    break;
                case GamePhase.Paused:
                    DrawPause(c);
                    break;
                case GamePhase.GameOver:
                    DrawGameOver(c, game.Score, ctx.Scores);
                    break;
            }
        }

        private void DrawBackground(DrawingContext c, Size size, double backgroundShift)
        {
            var rect = new Rect(0, 0, size.Width, size.Height);
            c.DrawRectangle(Linear(Rgb(15, 18, 30), Rgb(28, 38, 58), GradientDir.Diagonal), null, rect);

            c.DrawEllipse(Solid(30, 120, 180, 255), null, new Point(150, 10), 190, 110);
            c.DrawEllipse(Solid(26, 90, 255, 210), null,
                new Point(size.Width - 120, size.Height - 100), 160, 120);
        }

        private void DrawHud(DrawingContext c, Size size, int score, int best, string mode)
        {
            var hud = new Rect(14, 12, size.Width - 28, 52);

            c.DrawRectangle(Solid(35, 0, 0, 0), null,
                new Rect(hud.X, hud.Y + 3, hud.Width, hud.Height), 20, 20);

            c.DrawRectangle(
                Linear(Rgb(40, 48, 74), Rgb(24, 28, 42), GradientDir.Horizontal),
                new Pen(Solid(90, 170, 220, 255), 1.2), hud, 20, 20);

            IBrush label = Solid(255, 180, 195, 225);
            IBrush value = Solid(255, 110, 255, 210);

            c.DrawText(Text("SNAKE", Pt(16), Solid(255, 240, 245, 255), FontWeight.Bold), new Point(28, 20));
            c.DrawText(Text("Score", Pt(9), label), new Point(176, 17));
            c.DrawText(Text(score.ToString(), Pt(12), value, FontWeight.Bold), new Point(176, 33));
            c.DrawText(Text("Best", Pt(9), label), new Point(250, 17));
            c.DrawText(Text(best.ToString(), Pt(12), value, FontWeight.Bold), new Point(250, 33));
            c.DrawText(Text("Tryb", Pt(9), label), new Point(322, 17));
            c.DrawText(Text(mode, Pt(11), value, FontWeight.Bold), new Point(322, 34));
            c.DrawText(Text("WASD / strzałki  •  P = pauza  •  R = restart", Pt(8), label), new Point(420, 31));
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

        private void DrawObstacles(DrawingContext c, SnakeGame game)
        {
            foreach (PixelPoint cell in game.Obstacles)
            {
                Rect r = BoardGeometry.CellToRect(cell, 3);
                c.DrawRectangle(
                    Linear(Rgb(74, 82, 100), Rgb(44, 50, 64), GradientDir.Vertical),
                    new Pen(Solid(120, 160, 180, 210), 1), r, 6, 6);
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

        private void DrawFood(DrawingContext c, PixelPoint food, FoodType kind, double foodPulse)
        {
            Rect foodRect = BoardGeometry.CellToRect(food, 5);
            double pulse = Math.Sin(foodPulse) * 3.5;

            (Color from, Color to, IBrush glow) = kind switch
            {
                FoodType.Bonus => (Color.FromArgb(255, 255, 225, 130), Color.FromArgb(255, 255, 190, 40), Solid(80, 255, 210, 90)),
                FoodType.Slow => (Color.FromArgb(255, 160, 215, 255), Color.FromArgb(255, 70, 150, 255), Solid(80, 90, 160, 255)),
                _ => (Color.FromArgb(255, 255, 120, 155), Color.FromArgb(255, 255, 65, 90), Solid(65, 255, 100, 140))
            };

            c.DrawRectangle(glow, null, Inflate(foodRect, 8 + pulse), 16, 16);
            c.DrawRectangle(
                Linear(from, to, GradientDir.Diagonal),
                new Pen(Solid(170, 255, 235, 235), 1.2), foodRect, 12, 12);

            Dot(c, Solid(150, 255, 255, 255), foodRect.X + 5, foodRect.Y + 4, 6);
        }

        private void DrawReady(DrawingContext c, Difficulty difficulty, IReadOnlyList<int> scores)
        {
            Rect board = BoardGeometry.BoardRect;
            double height = 372;
            var overlay = new Rect(board.X + 35, board.Y + (board.Height - height) / 2, board.Width - 70, height);
            Card(c, overlay, Color.FromArgb(120, 110, 255, 210));

            double cx = overlay.X + overlay.Width / 2;
            double y = overlay.Y + 24;

            DrawCentered(c, Text("SNAKE", Pt(26), TitleBrush, FontWeight.Bold), cx, y); y += 52;
            DrawCentered(c, Text("Wybierz poziom trudności", Pt(11), DimBrush), cx, y); y += 28;

            DrawOption(c, cx, ref y, "1", "Łatwy — z owijaniem ścian", difficulty == Difficulty.Easy);
            DrawOption(c, cx, ref y, "2", "Normalny", difficulty == Difficulty.Normal);
            DrawOption(c, cx, ref y, "3", "Trudny — przeszkody, szybciej", difficulty == Difficulty.Hard);

            y += 12;
            DrawCentered(c, Text("ENTER lub strzałka — start", Pt(13), AccentBrush, FontWeight.Bold), cx, y); y += 38;
            DrawCentered(c, Text("Najlepsze wyniki", Pt(11), DimBrush), cx, y); y += 24;
            DrawScores(c, cx, ref y, scores);
        }

        private void DrawPause(DrawingContext c)
        {
            Rect board = BoardGeometry.BoardRect;
            double height = 150;
            var overlay = new Rect(board.X + 55, board.Y + (board.Height - height) / 2, board.Width - 110, height);
            Card(c, overlay, Color.FromArgb(120, 110, 255, 210));

            double cx = overlay.X + overlay.Width / 2;
            DrawCentered(c, Text("Pauza", Pt(24), TitleBrush, FontWeight.Bold), cx, overlay.Y + 32);
            DrawCentered(c, Text("P / ESC — wznów grę", Pt(13), AccentBrush, FontWeight.Bold), cx, overlay.Y + 88);
        }

        private void DrawGameOver(DrawingContext c, int score, IReadOnlyList<int> scores)
        {
            Rect board = BoardGeometry.BoardRect;
            double height = 320;
            var overlay = new Rect(board.X + 50, board.Y + (board.Height - height) / 2, board.Width - 100, height);
            Card(c, overlay, Color.FromArgb(120, 255, 120, 150));

            double cx = overlay.X + overlay.Width / 2;
            double y = overlay.Y + 26;

            DrawCentered(c, Text("Game Over", Pt(24), TitleBrush, FontWeight.Bold), cx, y); y += 50;
            DrawCentered(c, Text($"Wynik: {score}", Pt(14), AccentBrush, FontWeight.Bold), cx, y); y += 38;
            DrawCentered(c, Text("Najlepsze wyniki", Pt(11), DimBrush), cx, y); y += 24;
            DrawScores(c, cx, ref y, scores);
            y += 8;
            DrawCentered(c, Text("ENTER / SPACJA — nowa gra    •    R — restart", Pt(11), DimBrush), cx, y);
        }

        private void DrawOption(DrawingContext c, double cx, ref double y, string key, string label, bool selected)
        {
            IBrush brush = selected ? AccentBrush : FadedBrush;
            string prefix = selected ? "> " : "   ";
            FontWeight weight = selected ? FontWeight.Bold : FontWeight.Normal;
            DrawCentered(c, Text($"{prefix}[{key}]  {label}", Pt(12), brush, weight), cx, y);
            y += 28;
        }

        private void DrawScores(DrawingContext c, double cx, ref double y, IReadOnlyList<int> scores)
        {
            if (scores.Count == 0)
            {
                DrawCentered(c, Text("— brak wyników —", Pt(11), FadedBrush), cx, y);
                y += 22;
                return;
            }

            for (int i = 0; i < scores.Count; i++)
            {
                DrawCentered(c, Text($"{i + 1}.   {scores[i]}", Pt(11), i == 0 ? AccentBrush : DimBrush), cx, y);
                y += 22;
            }
        }

        // ----- pomocnicze -----

        private static IBrush TitleBrush => Solid(255, 245, 245, 250);
        private static IBrush AccentBrush => Solid(255, 110, 255, 210);
        private static IBrush DimBrush => Solid(205, 200, 210, 228);
        private static IBrush FadedBrush => Solid(150, 170, 180, 205);

        private static void Card(DrawingContext c, Rect overlay, Color border)
            => c.DrawRectangle(Solid(214, 12, 16, 24), new Pen(new SolidColorBrush(border), 1.5), overlay, 26, 26);

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
