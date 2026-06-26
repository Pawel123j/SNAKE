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
    /// (<see cref="GameRenderer"/>), obsługuje czasomierze, klawiaturę, dźwięki,
    /// wybór poziomu trudności i tabelę wyników.
    /// </summary>
    public sealed class GameControl : Control
    {
        private readonly DispatcherTimer gameTimer;
        private readonly DispatcherTimer pulseTimer;
        private readonly GameRenderer renderer = new GameRenderer();
        private readonly Scoreboard scoreboard = new Scoreboard();
        private readonly SoundManager sound = new SoundManager();
        private SnakeGame game;

        private GamePhase phase = GamePhase.Ready;
        private Difficulty difficulty = Difficulty.Normal;
        private double foodPulse;
        private double backgroundShift;

        public GameControl()
        {
            Width = GameConfig.BoardPixelWidth;
            Height = GameConfig.BoardPixelHeight;
            Focusable = true;

            scoreboard.Load();
            game = new SnakeGame(DifficultySettings.For(difficulty));

            gameTimer = new DispatcherTimer();
            gameTimer.Tick += (_, _) => OnGameTick();

            // Osobny czasomierz animacji (puls jedzenia, ruch tła) działa zawsze —
            // także na ekranie startowym i pauzie.
            pulseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            pulseTimer.Tick += (_, _) =>
            {
                foodPulse += 0.10;
                backgroundShift += 0.008;
                InvalidateVisual();
            };
            pulseTimer.Start();

            // Po dodaniu do okna przejmujemy fokus, żeby łapać klawisze.
            Loaded += (_, _) => Focus();
        }

        private void StartNewGame()
        {
            DifficultySettings settings = DifficultySettings.For(difficulty);
            game.Reset(settings);
            gameTimer.Interval = TimeSpan.FromMilliseconds(settings.StartIntervalMs);
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
                    ApplyFoodEffect();
                    break;
                case StepResult.Died:
                    EndGame();
                    break;
            }

            InvalidateVisual();
        }

        private void ApplyFoodEffect()
        {
            double interval = gameTimer.Interval.TotalMilliseconds;
            DifficultySettings settings = game.Settings;

            switch (game.LastEaten)
            {
                case FoodType.Bonus:
                    sound.Bonus();
                    interval = Math.Max(settings.MinIntervalMs, interval - 1);
                    break;

                case FoodType.Slow:
                    sound.Slow();
                    interval = Math.Min(settings.StartIntervalMs, interval + 14);
                    break;

                default:
                    sound.Eat();
                    interval = Math.Max(settings.MinIntervalMs, interval - 1);
                    break;
            }

            gameTimer.Interval = TimeSpan.FromMilliseconds(interval);
        }

        private void EndGame()
        {
            phase = GamePhase.GameOver;
            gameTimer.Stop();
            scoreboard.Add(game.Score);
            sound.GameOver();
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
            if (TrySelectDifficulty(e.Key))
                return;

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

        private bool TrySelectDifficulty(Key key)
        {
            Difficulty? chosen = key switch
            {
                Key.D1 or Key.NumPad1 => Difficulty.Easy,
                Key.D2 or Key.NumPad2 => Difficulty.Normal,
                Key.D3 or Key.NumPad3 => Difficulty.Hard,
                _ => null
            };

            if (chosen is null)
                return false;

            difficulty = chosen.Value;
            game.Reset(DifficultySettings.For(difficulty)); // odśwież podgląd planszy
            InvalidateVisual();
            return true;
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
            var ctx = new RenderContext(scoreboard.Scores, foodPulse, backgroundShift, phase, difficulty);
            renderer.Draw(context, new Size(GameConfig.BoardPixelWidth, GameConfig.BoardPixelHeight), game, ctx);
        }
    }
}
