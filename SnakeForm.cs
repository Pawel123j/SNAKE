using System;
using System.Drawing;
using System.Windows.Forms;

namespace ElegantSnake
{
    /// <summary>
    /// Okno gry — spina logikę (<see cref="SnakeGame"/>) z rysowaniem
    /// (<see cref="GameRenderer"/>), obsługuje czasomierze, klawiaturę i stany ekranów.
    /// </summary>
    public sealed class SnakeForm : Form
    {
        private readonly Timer gameTimer = new Timer();
        private readonly Timer pulseTimer = new Timer();
        private readonly SnakeGame game = new SnakeGame();
        private readonly GameRenderer renderer = new GameRenderer();

        private GamePhase phase = GamePhase.Ready;
        private int bestScore;
        private float foodPulse;
        private float backgroundShift;

        public SnakeForm()
        {
            Text = "Elegant Snake";
            ClientSize = new Size(
                GameConfig.GridSize * GameConfig.CellSize + GameConfig.BorderPadding * 2,
                GameConfig.GridSize * GameConfig.CellSize + GameConfig.BorderPadding * 2 + GameConfig.TopHudHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.FromArgb(10, 12, 18);
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);

            gameTimer.Interval = GameConfig.StartIntervalMs;
            gameTimer.Tick += (_, __) => OnGameTick();

            // Osobny czasomierz dla animacji (puls jedzenia, ruch tła) działa
            // zawsze — także na ekranie startowym i pauzie.
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
                bestScore = HighScoreStore.Load();
                phase = GamePhase.Ready;
                pulseTimer.Start();
                Invalidate();
            };
        }

        private void StartNewGame()
        {
            game.Reset();
            gameTimer.Interval = GameConfig.StartIntervalMs;
            phase = GamePhase.Playing;
            gameTimer.Start();
            Invalidate();
        }

        private void OnGameTick()
        {
            if (phase != GamePhase.Playing)
                return;

            StepResult result = game.Step();

            switch (result)
            {
                case StepResult.Ate:
                    if (game.Score > bestScore)
                        bestScore = game.Score;

                    // Każdy zjedzony owoc lekko przyspiesza grę.
                    if (gameTimer.Interval > GameConfig.MinIntervalMs)
                        gameTimer.Interval -= 1;
                    break;

                case StepResult.Died:
                    EndGame();
                    break;
            }

            Invalidate();
        }

        private void EndGame()
        {
            phase = GamePhase.GameOver;
            gameTimer.Stop();

            if (game.Score > bestScore)
                bestScore = game.Score;

            HighScoreStore.Save(bestScore);
            Invalidate();
        }

        private void TogglePause()
        {
            if (phase == GamePhase.Playing)
            {
                phase = GamePhase.Paused;
                gameTimer.Stop();
            }
            else if (phase == GamePhase.Paused)
            {
                phase = GamePhase.Playing;
                gameTimer.Start();
            }

            Invalidate();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            switch (phase)
            {
                case GamePhase.Ready:
                    HandleReadyKeys(e);
                    break;

                case GamePhase.Playing:
                    HandlePlayingKeys(e);
                    break;

                case GamePhase.Paused:
                    if (e.KeyCode is Keys.P or Keys.Escape)
                        TogglePause();
                    break;

                case GamePhase.GameOver:
                    if (e.KeyCode is Keys.Enter or Keys.Space or Keys.R)
                        StartNewGame();
                    break;
            }
        }

        private void HandleReadyKeys(KeyEventArgs e)
        {
            if (TryGetDirection(e.KeyCode, out Direction direction))
            {
                StartNewGame();
                game.SetDirection(direction);
            }
            else if (e.KeyCode is Keys.Enter or Keys.Space)
            {
                StartNewGame();
            }
        }

        private void HandlePlayingKeys(KeyEventArgs e)
        {
            if (TryGetDirection(e.KeyCode, out Direction direction))
                game.SetDirection(direction);
            else if (e.KeyCode is Keys.P or Keys.Escape)
                TogglePause();
            else if (e.KeyCode == Keys.R)
                StartNewGame();
        }

        private static bool TryGetDirection(Keys key, out Direction direction)
        {
            switch (key)
            {
                case Keys.Up:
                case Keys.W:
                    direction = Direction.Up;
                    return true;
                case Keys.Down:
                case Keys.S:
                    direction = Direction.Down;
                    return true;
                case Keys.Left:
                case Keys.A:
                    direction = Direction.Left;
                    return true;
                case Keys.Right:
                case Keys.D:
                    direction = Direction.Right;
                    return true;
                default:
                    direction = Direction.Right;
                    return false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var ctx = new RenderContext(bestScore, foodPulse, backgroundShift, phase);
            renderer.Draw(e.Graphics, ClientRectangle, game, ctx);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                gameTimer.Dispose();
                pulseTimer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
