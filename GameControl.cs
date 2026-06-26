using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace ElegantSnake
{
    /// <summary>
    /// Kontrolka gry — spina logikę (<see cref="SnakeGame"/>) z rysowaniem
    /// (<see cref="GameRenderer"/>), obsługuje czasomierze, klawiaturę i stany ekranów.
    /// </summary>
    public sealed class GameControl : Control
    {
        private readonly DispatcherTimer gameTimer;
        private readonly DispatcherTimer pulseTimer;
        private readonly SnakeGame game = new SnakeGame();
        private readonly GameRenderer renderer = new GameRenderer();

        private GamePhase phase = GamePhase.Ready;
        private int bestScore;
        private double foodPulse;
        private double backgroundShift;

        public GameControl()
        {
            Width = GameConfig.BoardPixelWidth;
            Height = GameConfig.BoardPixelHeight;
            Focusable = true;

            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(GameConfig.StartIntervalMs)
            };
            gameTimer.Tick += (_, _) => OnGameTick();

            // Osobny czasomierz animacji (puls jedzenia, ruch tła) działa zawsze —
            // także na ekranie startowym i pauzie.
            pulseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            pulseTimer.Tick += (_, _) =>
            {
                foodPulse += 0.10;
                backgroundShift += 0.008;
                InvalidateVisual();
            };

            bestScore = HighScoreStore.Load();
            pulseTimer.Start();

            // Po dodaniu do okna przejmujemy fokus, żeby łapać klawisze.
            Loaded += (_, _) => Focus();
        }

        private void StartNewGame()
        {
            game.Reset();
            gameTimer.Interval = TimeSpan.FromMilliseconds(GameConfig.StartIntervalMs);
            phase = GamePhase.Playing;
            gameTimer.Start();
            InvalidateVisual();
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
                    double current = gameTimer.Interval.TotalMilliseconds;
                    if (current > GameConfig.MinIntervalMs)
                        gameTimer.Interval = TimeSpan.FromMilliseconds(current - 1);
                    break;

                case StepResult.Died:
                    EndGame();
                    break;
            }

            InvalidateVisual();
        }

        private void EndGame()
        {
            phase = GamePhase.GameOver;
            gameTimer.Stop();

            if (game.Score > bestScore)
                bestScore = game.Score;

            HighScoreStore.Save(bestScore);
            InvalidateVisual();
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

            InvalidateVisual();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (phase)
            {
                case GamePhase.Ready:
                    HandleReadyKeys(e);
                    break;

                case GamePhase.Playing:
                    HandlePlayingKeys(e);
                    break;

                case GamePhase.Paused:
                    if (e.Key is Key.P or Key.Escape)
                        TogglePause();
                    break;

                case GamePhase.GameOver:
                    if (e.Key is Key.Enter or Key.Space or Key.R)
                        StartNewGame();
                    break;
            }

            e.Handled = true;
        }

        private void HandleReadyKeys(KeyEventArgs e)
        {
            if (TryGetDirection(e.Key, out Direction direction))
            {
                StartNewGame();
                game.SetDirection(direction);
            }
            else if (e.Key is Key.Enter or Key.Space)
            {
                StartNewGame();
            }
        }

        private void HandlePlayingKeys(KeyEventArgs e)
        {
            if (TryGetDirection(e.Key, out Direction direction))
                game.SetDirection(direction);
            else if (e.Key is Key.P or Key.Escape)
                TogglePause();
            else if (e.Key == Key.R)
                StartNewGame();
        }

        private static bool TryGetDirection(Key key, out Direction direction)
        {
            switch (key)
            {
                case Key.Up:
                case Key.W:
                    direction = Direction.Up;
                    return true;
                case Key.Down:
                case Key.S:
                    direction = Direction.Down;
                    return true;
                case Key.Left:
                case Key.A:
                    direction = Direction.Left;
                    return true;
                case Key.Right:
                case Key.D:
                    direction = Direction.Right;
                    return true;
                default:
                    direction = Direction.Right;
                    return false;
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var ctx = new RenderContext(bestScore, foodPulse, backgroundShift, phase);
            renderer.Draw(context, new Size(GameConfig.BoardPixelWidth, GameConfig.BoardPixelHeight), game, ctx);
        }
    }
}
