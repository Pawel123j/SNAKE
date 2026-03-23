using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ElegantSnake
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SnakeForm());
        }
    }

    public class SnakeForm : Form
    {
        private readonly Timer gameTimer = new Timer();
        private readonly Timer pulseTimer = new Timer();
        private readonly Random random = new Random();

        private readonly List<Point> snake = new List<Point>();
        private Point food;
        private Direction currentDirection = Direction.Right;
        private Direction nextDirection = Direction.Right;

        private const int GridSize = 26;
        private const int CellSize = 24;
        private const int TopHudHeight = 78;
        private const int BorderPadding = 18;

        private int score;
        private int bestScore;
        private bool gameOver;
        private float foodPulse = 0f;
        private float backgroundShift = 0f;

        public SnakeForm()
        {
            Text = "Elegant Snake";
            ClientSize = new Size(
                GridSize * CellSize + BorderPadding * 2,
                GridSize * CellSize + BorderPadding * 2 + TopHudHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.FromArgb(10, 12, 18);
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);

            gameTimer.Interval = 95;
            gameTimer.Tick += (_, __) => UpdateGame();

            pulseTimer.Interval = 16;
            pulseTimer.Tick += (_, __) =>
            {
                foodPulse += 0.10f;
                backgroundShift += 0.008f;
                Invalidate();
            };

            KeyDown += OnKeyDown;
            Shown += (_, __) =>
            {
                ResetGame();
                pulseTimer.Start();
            };
        }

        private Rectangle BoardRect => new Rectangle(
            BorderPadding,
            TopHudHeight,
            GridSize * CellSize,
            GridSize * CellSize);

        private void ResetGame()
        {
            snake.Clear();
            snake.Add(new Point(8, 12));
            snake.Add(new Point(7, 12));
            snake.Add(new Point(6, 12));
            snake.Add(new Point(5, 12));

            currentDirection = Direction.Right;
            nextDirection = Direction.Right;
            score = 0;
            gameOver = false;
            SpawnFood();
            gameTimer.Start();
            Invalidate();
        }

        private void SpawnFood()
        {
            do
            {
                food = new Point(random.Next(0, GridSize), random.Next(0, GridSize));
            }
            while (snake.Contains(food));
        }

        private void UpdateGame()
        {
            if (gameOver)
                return;

            currentDirection = nextDirection;

            Point head = snake[0];
            Point newHead = currentDirection switch
            {
                Direction.Up => new Point(head.X, head.Y - 1),
                Direction.Down => new Point(head.X, head.Y + 1),
                Direction.Left => new Point(head.X - 1, head.Y),
                _ => new Point(head.X + 1, head.Y)
            };

            if (newHead.X < 0 || newHead.X >= GridSize || newHead.Y < 0 || newHead.Y >= GridSize)
            {
                EndGame();
                return;
            }

            bool grows = newHead == food;
            var bodyToCheck = grows ? snake : snake.Take(snake.Count - 1);
            if (bodyToCheck.Contains(newHead))
            {
                EndGame();
                return;
            }

            snake.Insert(0, newHead);

            if (grows)
            {
                score += 10;
                bestScore = Math.Max(bestScore, score);
                SpawnFood();

                if (gameTimer.Interval > 55)
                    gameTimer.Interval -= 1;
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            Invalidate();
        }

        private void EndGame()
        {
            gameOver = true;
            gameTimer.Stop();
            bestScore = Math.Max(bestScore, score);
            Invalidate();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (gameOver && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
            {
                gameTimer.Interval = 95;
                ResetGame();
                return;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
            {
                if (currentDirection != Direction.Down)
                    nextDirection = Direction.Up;
            }
            else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
            {
                if (currentDirection != Direction.Up)
                    nextDirection = Direction.Down;
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                if (currentDirection != Direction.Right)
                    nextDirection = Direction.Left;
            }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                if (currentDirection != Direction.Left)
                    nextDirection = Direction.Right;
            }
            else if (e.KeyCode == Keys.R)
            {
                gameTimer.Interval = 95;
                ResetGame();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            DrawBackground(g);
            DrawHud(g);
            DrawBoard(g);
            DrawFood(g);
            DrawSnake(g);

            if (gameOver)
                DrawGameOver(g);
        }

        private void DrawBackground(Graphics g)
        {
            using var backgroundBrush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(15, 18, 30),
                Color.FromArgb(28, 38, 58),
                35f + backgroundShift * 50f);
            g.FillRectangle(backgroundBrush, ClientRectangle);

            using var topGlow = new SolidBrush(Color.FromArgb(30, 120, 180, 255));
            g.FillEllipse(topGlow, -40, -100, 380, 220);

            using var bottomGlow = new SolidBrush(Color.FromArgb(26, 90, 255, 210));
            g.FillEllipse(bottomGlow, Width - 280, Height - 220, 320, 240);
        }

        private void DrawHud(Graphics g)
        {
            Rectangle hudRect = new Rectangle(14, 12, ClientSize.Width - 28, 52);
            using GraphicsPath hudPath = CreateRoundedRect(hudRect, 20);

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
            g.DrawString("Sterowanie: WASD / strzałki   •   R = restart", smallFont, labelBrush, 360, 31);
        }

        private void DrawBoard(Graphics g)
        {
            Rectangle board = BoardRect;
            Rectangle shadowRect = new Rectangle(board.X, board.Y + 5, board.Width, board.Height);

            using GraphicsPath shadowPath = CreateRoundedRect(shadowRect, 26);
            using var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
            g.FillPath(shadowBrush, shadowPath);

            using GraphicsPath boardPath = CreateRoundedRect(board, 26);
            using var boardBrush = new LinearGradientBrush(
                board,
                Color.FromArgb(14, 16, 26),
                Color.FromArgb(18, 24, 36),
                90f);
            g.FillPath(boardBrush, boardPath);

            using var borderPen = new Pen(Color.FromArgb(80, 165, 195, 255), 1.4f);
            g.DrawPath(borderPen, boardPath);

            using var gridPen = new Pen(Color.FromArgb(18, 220, 235, 255), 1f);

            for (int x = 1; x < GridSize; x++)
            {
                int px = board.X + x * CellSize;
                g.DrawLine(gridPen, px, board.Y + 10, px, board.Bottom - 10);
            }

            for (int y = 1; y < GridSize; y++)
            {
                int py = board.Y + y * CellSize;
                g.DrawLine(gridPen, board.X + 10, py, board.Right - 10, py);
            }
        }

        private void DrawSnake(Graphics g)
        {
            for (int i = snake.Count - 1; i >= 0; i--)
            {
                var segment = snake[i];
                Rectangle cell = CellToRect(segment, 4);
                int alphaBoost = Math.Min(70 + i * 8, 185);

                Color start;
                Color end;
                int radius;

                if (i == 0)
                {
                    start = Color.FromArgb(255, 125, 255, 220);
                    end = Color.FromArgb(255, 45, 210, 160);
                    radius = 12;

                    Rectangle glow = InflateRect(cell, 6);
                    using GraphicsPath glowPath = CreateRoundedRect(glow, 16);
                    using var glowBrush = new SolidBrush(Color.FromArgb(55, 100, 255, 220));
                    g.FillPath(glowBrush, glowPath);
                }
                else
                {
                    start = Color.FromArgb(alphaBoost, 90, 220, 185);
                    end = Color.FromArgb(alphaBoost, 35, 150, 125);
                    radius = 10;
                }

                using GraphicsPath path = CreateRoundedRect(cell, radius);
                using var segmentBrush = new LinearGradientBrush(cell, start, end, 45f);
                g.FillPath(segmentBrush, path);

                using var borderPen = new Pen(Color.FromArgb(90, 230, 255, 245), 1f);
                g.DrawPath(borderPen, path);

                if (i == 0)
                    DrawSnakeFace(g, cell);
            }
        }

        private void DrawSnakeFace(Graphics g, Rectangle headRect)
        {
            int eyeSize = 4;
            int eyeY = headRect.Y + 7;
            int leftEyeX = headRect.X + 7;
            int rightEyeX = headRect.Right - 11;

            if (currentDirection == Direction.Left || currentDirection == Direction.Right)
            {
                eyeY = headRect.Y + 6;
                leftEyeX = currentDirection == Direction.Right ? headRect.Right - 11 : headRect.X + 7;
                rightEyeX = leftEyeX;
            }

            using var eyeBrush = new SolidBrush(Color.FromArgb(230, 10, 24, 28));

            if (currentDirection == Direction.Up || currentDirection == Direction.Down)
            {
                g.FillEllipse(eyeBrush, leftEyeX, eyeY, eyeSize, eyeSize);
                g.FillEllipse(eyeBrush, rightEyeX, eyeY, eyeSize, eyeSize);
            }
            else
            {
                int eyeX = currentDirection == Direction.Right ? headRect.Right - 10 : headRect.X + 6;
                g.FillEllipse(eyeBrush, eyeX, headRect.Y + 7, eyeSize, eyeSize);
                g.FillEllipse(eyeBrush, eyeX, headRect.Bottom - 11, eyeSize, eyeSize);
            }
        }

        private void DrawFood(Graphics g)
        {
            Rectangle foodRect = CellToRect(food, 5);
            int pulse = (int)(Math.Sin(foodPulse) * 3.5f);
            Rectangle glowRect = InflateRect(foodRect, 8 + pulse);

            using GraphicsPath glowPath = CreateRoundedRect(glowRect, 16);
            using var glowBrush = new SolidBrush(Color.FromArgb(65, 255, 100, 140));
            g.FillPath(glowBrush, glowPath);

            using GraphicsPath foodPath = CreateRoundedRect(foodRect, 12);
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

        private void DrawGameOver(Graphics g)
        {
            Rectangle overlay = new Rectangle(BoardRect.X + 65, BoardRect.Y + 150, BoardRect.Width - 130, 180);
            using GraphicsPath overlayPath = CreateRoundedRect(overlay, 28);
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

            g.DrawString("Game Over", titleFont, titleBrush, overlay.X + overlay.Width / 2, overlay.Y + 28, sfCenter);
            g.DrawString($"Wynik: {score}", valueFont, accentBrush, overlay.X + overlay.Width / 2, overlay.Y + 85, sfCenter);
            g.DrawString("ENTER albo SPACJA — nowa gra", textFont, textBrush, overlay.X + overlay.Width / 2, overlay.Y + 120, sfCenter);
            g.DrawString("R — szybki restart", textFont, textBrush, overlay.X + overlay.Width / 2, overlay.Y + 145, sfCenter);
        }

        private Rectangle CellToRect(Point cell, int inset)
        {
            Rectangle board = BoardRect;
            return new Rectangle(
                board.X + cell.X * CellSize + inset,
                board.Y + cell.Y * CellSize + inset,
                CellSize - inset * 2,
                CellSize - inset * 2);
        }

        private static Rectangle InflateRect(Rectangle rect, int amount)
        {
            return new Rectangle(rect.X - amount, rect.Y - amount, rect.Width + amount * 2, rect.Height + amount * 2);
        }

        private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}