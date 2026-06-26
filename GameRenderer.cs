using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ElegantSnake
{
    /// <summary>Dane potrzebne rendererowi do narysowania jednej klatki.</summary>
    public readonly struct RenderContext
    {
        public RenderContext(int bestScore, float foodPulse, float backgroundShift, GamePhase phase)
        {
            BestScore = bestScore;
            FoodPulse = foodPulse;
            BackgroundShift = backgroundShift;
            Phase = phase;
        }

        public int BestScore { get; }
        public float FoodPulse { get; }
        public float BackgroundShift { get; }
        public GamePhase Phase { get; }
    }

    /// <summary>
    /// Odpowiada za całe rysowanie gry — tło, HUD, planszę, węża, jedzenie
    /// oraz nakładki ekranów (start, pauza, koniec gry).
    /// </summary>
    public sealed class GameRenderer
    {
        /// <summary>Rysuje całą klatkę gry.</summary>
        public void Draw(Graphics g, Rectangle clientRect, SnakeGame game, RenderContext ctx)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            DrawBackground(g, clientRect, ctx.BackgroundShift);
            DrawHud(g, clientRect, game.Score, ctx.BestScore);
            DrawBoard(g);
            DrawFood(g, game.Food, ctx.FoodPulse);
            DrawSnake(g, game);

            switch (ctx.Phase)
            {
                case GamePhase.Ready:
                    DrawCenteredCard(g, "SNAKE", "ENTER lub strzałka — start",
                        "Sterowanie: WASD / strzałki   •   P = pauza");
                    break;
                case GamePhase.Paused:
                    DrawCenteredCard(g, "Pauza", "P / ESC — wznów grę", null);
                    break;
                case GamePhase.GameOver:
                    DrawGameOver(g, game.Score);
                    break;
            }
        }

        private void DrawBackground(Graphics g, Rectangle clientRect, float backgroundShift)
        {
            using var backgroundBrush = new LinearGradientBrush(
                clientRect,
                Color.FromArgb(15, 18, 30),
                Color.FromArgb(28, 38, 58),
                35f + backgroundShift * 50f);
            g.FillRectangle(backgroundBrush, clientRect);

            using var topGlow = new SolidBrush(Color.FromArgb(30, 120, 180, 255));
            g.FillEllipse(topGlow, -40, -100, 380, 220);

            using var bottomGlow = new SolidBrush(Color.FromArgb(26, 90, 255, 210));
            g.FillEllipse(bottomGlow, clientRect.Width - 280, clientRect.Height - 220, 320, 240);
        }

        private void DrawHud(Graphics g, Rectangle clientRect, int score, int bestScore)
        {
            Rectangle hudRect = new Rectangle(14, 12, clientRect.Width - 28, 52);
            using GraphicsPath hudPath = GraphicsUtils.CreateRoundedRect(hudRect, 20);

            using var hudShadow = new SolidBrush(Color.FromArgb(35, 0, 0, 0));
            g.TranslateTransform(0, 3);
            g.FillPath(hudShadow, hudPath);
            g.ResetTransform();

            using var hudBrush = new LinearGradientBrush(
                hudRect,
                Color.FromArgb(40, 48, 74),
                Color.FromArgb(24, 28, 42),
                0f);
            g.FillPath(hudBrush, hudPath);

            using var hudBorder = new Pen(Color.FromArgb(90, 170, 220, 255), 1.2f);
            g.DrawPath(hudBorder, hudPath);

            using var titleBrush = new SolidBrush(Color.FromArgb(240, 245, 255));
            using var labelBrush = new SolidBrush(Color.FromArgb(180, 195, 225));
            using var valueBrush = new SolidBrush(Color.FromArgb(110, 255, 210));

            using var titleFont = new Font("Segoe UI Semibold", 16f, FontStyle.Bold);
            using var smallFont = new Font("Segoe UI", 9f, FontStyle.Regular);
            using var scoreFont = new Font("Segoe UI Semibold", 12f, FontStyle.Bold);

            g.DrawString("SNAKE", titleFont, titleBrush, 28, 24);
            g.DrawString("Score", smallFont, labelBrush, 190, 22);
            g.DrawString(score.ToString(), scoreFont, valueBrush, 190, 37);
            g.DrawString("Best", smallFont, labelBrush, 270, 22);
            g.DrawString(bestScore.ToString(), scoreFont, valueBrush, 270, 37);
            g.DrawString("Sterowanie: WASD / strzałki   •   P = pauza   •   R = restart",
                smallFont, labelBrush, 360, 31);
        }

        private void DrawBoard(Graphics g)
        {
            Rectangle board = BoardGeometry.BoardRect;
            Rectangle shadowRect = new Rectangle(board.X, board.Y + 5, board.Width, board.Height);

            using GraphicsPath shadowPath = GraphicsUtils.CreateRoundedRect(shadowRect, 26);
            using var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
            g.FillPath(shadowBrush, shadowPath);

            using GraphicsPath boardPath = GraphicsUtils.CreateRoundedRect(board, 26);
            using var boardBrush = new LinearGradientBrush(
                board,
                Color.FromArgb(14, 16, 26),
                Color.FromArgb(18, 24, 36),
                90f);
            g.FillPath(boardBrush, boardPath);

            using var borderPen = new Pen(Color.FromArgb(80, 165, 195, 255), 1.4f);
            g.DrawPath(borderPen, boardPath);

            using var gridPen = new Pen(Color.FromArgb(18, 220, 235, 255), 1f);

            for (int x = 1; x < GameConfig.GridSize; x++)
            {
                int px = board.X + x * GameConfig.CellSize;
                g.DrawLine(gridPen, px, board.Y + 10, px, board.Bottom - 10);
            }

            for (int y = 1; y < GameConfig.GridSize; y++)
            {
                int py = board.Y + y * GameConfig.CellSize;
                g.DrawLine(gridPen, board.X + 10, py, board.Right - 10, py);
            }
        }

        private void DrawSnake(Graphics g, SnakeGame game)
        {
            var snake = game.Snake;
            for (int i = snake.Count - 1; i >= 0; i--)
            {
                Point segment = snake[i];
                Rectangle cell = BoardGeometry.CellToRect(segment, 4);
                int alphaBoost = Math.Min(70 + i * 8, 185);

                Color start;
                Color end;
                int radius;

                if (i == 0)
                {
                    start = Color.FromArgb(255, 125, 255, 220);
                    end = Color.FromArgb(255, 45, 210, 160);
                    radius = 12;

                    Rectangle glow = GraphicsUtils.Inflate(cell, 6);
                    using GraphicsPath glowPath = GraphicsUtils.CreateRoundedRect(glow, 16);
                    using var glowBrush = new SolidBrush(Color.FromArgb(55, 100, 255, 220));
                    g.FillPath(glowBrush, glowPath);
                }
                else
                {
                    start = Color.FromArgb(alphaBoost, 90, 220, 185);
                    end = Color.FromArgb(alphaBoost, 35, 150, 125);
                    radius = 10;
                }

                using GraphicsPath path = GraphicsUtils.CreateRoundedRect(cell, radius);
                using var segmentBrush = new LinearGradientBrush(cell, start, end, 45f);
                g.FillPath(segmentBrush, path);

                using var borderPen = new Pen(Color.FromArgb(90, 230, 255, 245), 1f);
                g.DrawPath(borderPen, path);

                if (i == 0)
                    DrawSnakeFace(g, cell, game.CurrentDirection);
            }
        }

        private void DrawSnakeFace(Graphics g, Rectangle headRect, Direction direction)
        {
            int eyeSize = 4;
            int eyeY = headRect.Y + 7;
            int leftEyeX = headRect.X + 7;
            int rightEyeX = headRect.Right - 11;

            if (direction == Direction.Left || direction == Direction.Right)
            {
                eyeY = headRect.Y + 6;
                leftEyeX = direction == Direction.Right ? headRect.Right - 11 : headRect.X + 7;
                rightEyeX = leftEyeX;
            }

            using var eyeBrush = new SolidBrush(Color.FromArgb(230, 10, 24, 28));

            if (direction == Direction.Up || direction == Direction.Down)
            {
                g.FillEllipse(eyeBrush, leftEyeX, eyeY, eyeSize, eyeSize);
                g.FillEllipse(eyeBrush, rightEyeX, eyeY, eyeSize, eyeSize);
            }
            else
            {
                int eyeX = direction == Direction.Right ? headRect.Right - 10 : headRect.X + 6;
                g.FillEllipse(eyeBrush, eyeX, headRect.Y + 7, eyeSize, eyeSize);
                g.FillEllipse(eyeBrush, eyeX, headRect.Bottom - 11, eyeSize, eyeSize);
            }
        }

        private void DrawFood(Graphics g, Point food, float foodPulse)
        {
            Rectangle foodRect = BoardGeometry.CellToRect(food, 5);
            int pulse = (int)(Math.Sin(foodPulse) * 3.5f);
            Rectangle glowRect = GraphicsUtils.Inflate(foodRect, 8 + pulse);

            using GraphicsPath glowPath = GraphicsUtils.CreateRoundedRect(glowRect, 16);
            using var glowBrush = new SolidBrush(Color.FromArgb(65, 255, 100, 140));
            g.FillPath(glowBrush, glowPath);

            using GraphicsPath foodPath = GraphicsUtils.CreateRoundedRect(foodRect, 12);
            using var foodBrush = new LinearGradientBrush(
                foodRect,
                Color.FromArgb(255, 255, 120, 155),
                Color.FromArgb(255, 255, 65, 90),
                45f);
            g.FillPath(foodBrush, foodPath);

            using var foodBorder = new Pen(Color.FromArgb(170, 255, 230, 235), 1.2f);
            g.DrawPath(foodBorder, foodPath);

            using var shineBrush = new SolidBrush(Color.FromArgb(140, 255, 255, 255));
            g.FillEllipse(shineBrush, foodRect.X + 5, foodRect.Y + 4, 6, 6);
        }

        private void DrawGameOver(Graphics g, int score)
        {
            Rectangle board = BoardGeometry.BoardRect;
            Rectangle overlay = new Rectangle(board.X + 65, board.Y + 150, board.Width - 130, 180);
            using GraphicsPath overlayPath = GraphicsUtils.CreateRoundedRect(overlay, 28);
            using var overlayBrush = new SolidBrush(Color.FromArgb(210, 12, 16, 24));
            g.FillPath(overlayBrush, overlayPath);

            using var borderPen = new Pen(Color.FromArgb(120, 255, 120, 150), 1.5f);
            g.DrawPath(borderPen, overlayPath);

            using var titleFont = new Font("Segoe UI Semibold", 24f, FontStyle.Bold);
            using var textFont = new Font("Segoe UI", 12f, FontStyle.Regular);
            using var valueFont = new Font("Segoe UI Semibold", 14f, FontStyle.Bold);

            using var titleBrush = new SolidBrush(Color.FromArgb(255, 245, 245, 250));
            using var textBrush = new SolidBrush(Color.FromArgb(200, 205, 215, 230));
            using var accentBrush = new SolidBrush(Color.FromArgb(255, 110, 255, 210));

            var sfCenter = new StringFormat { Alignment = StringAlignment.Center };
            int cx = overlay.X + overlay.Width / 2;

            g.DrawString("Game Over", titleFont, titleBrush, cx, overlay.Y + 28, sfCenter);
            g.DrawString($"Wynik: {score}", valueFont, accentBrush, cx, overlay.Y + 85, sfCenter);
            g.DrawString("ENTER albo SPACJA — nowa gra", textFont, textBrush, cx, overlay.Y + 120, sfCenter);
            g.DrawString("R — szybki restart", textFont, textBrush, cx, overlay.Y + 145, sfCenter);
        }

        /// <summary>Uniwersalna nakładka z tytułem, podtytułem i opcjonalną podpowiedzią.</summary>
        private void DrawCenteredCard(Graphics g, string title, string subtitle, string? hint)
        {
            Rectangle board = BoardGeometry.BoardRect;
            int height = hint == null ? 150 : 185;
            Rectangle overlay = new Rectangle(
                board.X + 55,
                board.Y + (board.Height - height) / 2,
                board.Width - 110,
                height);

            using GraphicsPath overlayPath = GraphicsUtils.CreateRoundedRect(overlay, 28);
            using var overlayBrush = new SolidBrush(Color.FromArgb(210, 12, 16, 24));
            g.FillPath(overlayBrush, overlayPath);

            using var borderPen = new Pen(Color.FromArgb(120, 110, 255, 210), 1.5f);
            g.DrawPath(borderPen, overlayPath);

            using var titleFont = new Font("Segoe UI Semibold", 24f, FontStyle.Bold);
            using var subFont = new Font("Segoe UI Semibold", 13f, FontStyle.Bold);
            using var hintFont = new Font("Segoe UI", 11f, FontStyle.Regular);

            using var titleBrush = new SolidBrush(Color.FromArgb(255, 245, 245, 250));
            using var accentBrush = new SolidBrush(Color.FromArgb(255, 110, 255, 210));
            using var hintBrush = new SolidBrush(Color.FromArgb(200, 205, 215, 230));

            var sfCenter = new StringFormat { Alignment = StringAlignment.Center };
            int cx = overlay.X + overlay.Width / 2;

            g.DrawString(title, titleFont, titleBrush, cx, overlay.Y + 30, sfCenter);
            g.DrawString(subtitle, subFont, accentBrush, cx, overlay.Y + 84, sfCenter);
            if (hint != null)
                g.DrawString(hint, hintFont, hintBrush, cx, overlay.Y + 122, sfCenter);
        }
    }
}
