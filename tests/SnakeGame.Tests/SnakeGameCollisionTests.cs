using System.Linq;
using Avalonia;
using Xunit;

namespace ElegantSnake.Tests
{
    /// <summary>
    /// Kolizje, owijanie ścian, przeszkody i punktacja — czyli te zachowania,
    /// których zepsucie gracz odczuje natychmiast, a testy logiczne dotąd
    /// pokrywały tylko częściowo.
    /// </summary>
    public class SnakeGameCollisionTests
    {
        private static SnakeGame NewGame(Difficulty difficulty = Difficulty.Normal)
            => new SnakeGame(DifficultySettings.For(difficulty));

        /// <summary>
        /// Odkłada owoc daleko od toru węża, żeby przypadkowe zjedzenie nie
        /// zmieniło długości węża w trakcie testu.
        /// </summary>
        private static void ParkFood(SnakeGame game) => game.PlaceFood(new PixelPoint(0, 0), FoodType.Normal);

        // ── Kolizja z własnym ciałem ────────────────────────────────────────

        [Fact]
        public void Step_TurningIntoOwnBody_Dies()
        {
            var game = NewGame();

            // Karmimy węża raz, żeby urósł do pięciu segmentów — czterosegmentowy
            // wąż nie jest w stanie dogonić własnego ogona.
            PixelPoint head = game.Snake[0];
            game.PlaceFood(new PixelPoint(head.X + 1, head.Y), FoodType.Normal);
            Assert.Equal(StepResult.Ate, game.Step());
            Assert.Equal(5, game.Snake.Count);
            ParkFood(game);

            // Pętla w prawo-dół-lewo-góra prowadzi głowę na własny bok.
            game.SetDirection(Direction.Down);
            Assert.Equal(StepResult.Moved, game.Step());
            game.SetDirection(Direction.Left);
            Assert.Equal(StepResult.Moved, game.Step());
            game.SetDirection(Direction.Up);

            Assert.Equal(StepResult.Died, game.Step());
        }

        [Fact]
        public void Step_TailVacatesItsCell()
        {
            // Na tym stoi reguła, że wejście na pole opuszczane przez ogon
            // jest legalne: w kodzie kolizja sprawdzana jest wobec
            // `snake.Take(Count - 1)`, czyli z pominięciem ostatniego
            // segmentu. Ten test pilnuje przesłanki — że ogon faktycznie
            // znika z planszy w tym samym kroku.
            //
            // NIE sprawdza samego wejścia głowy na to pole: ustawienie węża
            // w taki układ wymaga pętli, której czterosegmentowy wąż nie
            // jest w stanie wykonać.
            var game = NewGame();
            ParkFood(game);

            PixelPoint tailBefore = game.Snake[game.Snake.Count - 1];
            Assert.Equal(StepResult.Moved, game.Step());

            Assert.DoesNotContain(tailBefore, game.Snake);
        }

        // ── Ściany ──────────────────────────────────────────────────────────

        [Fact]
        public void Step_HittingTopWall_Dies_WhenNoWrap()
        {
            var game = NewGame(Difficulty.Normal);
            ParkFood(game);
            game.SetDirection(Direction.Up);

            StepResult result = StepResult.Moved;
            for (int i = 0; i < GameConfig.GridSize + 5 && result != StepResult.Died; i++)
                result = game.Step();

            Assert.Equal(StepResult.Died, result);
        }

        [Fact]
        public void Step_WrapsVertically_OnEasy()
        {
            var game = NewGame(Difficulty.Easy);
            ParkFood(game);
            game.SetDirection(Direction.Up);

            // Wąż startuje w wierszu 12, więc po 13 krokach w górę musi
            // przejść przez górną krawędź.
            for (int i = 0; i < 20; i++)
                Assert.NotEqual(StepResult.Died, game.Step());

            Assert.InRange(game.Snake[0].Y, 0, GameConfig.GridSize - 1);
        }

        [Fact]
        public void Step_KeepsAllSegmentsOnBoard_WhenWrapping()
        {
            var game = NewGame(Difficulty.Easy);
            ParkFood(game);

            for (int i = 0; i < GameConfig.GridSize * 2; i++)
            {
                game.Step();
                foreach (PixelPoint segment in game.Snake)
                {
                    Assert.InRange(segment.X, 0, GameConfig.GridSize - 1);
                    Assert.InRange(segment.Y, 0, GameConfig.GridSize - 1);
                }
            }
        }

        // ── Przeszkody ──────────────────────────────────────────────────────

        [Fact]
        public void Hard_PlacesExpectedNumberOfObstacles()
        {
            var game = NewGame(Difficulty.Hard);
            Assert.Equal(DifficultySettings.For(Difficulty.Hard).ObstacleCount, game.Obstacles.Count);
        }

        [Fact]
        public void Obstacles_NeverBlockTheStartingRow()
        {
            // Wąż startuje w wierszu 12. Przeszkoda postawiona na jego torze
            // zabijałaby gracza, zanim ten zdąży nacisnąć klawisz.
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var game = NewGame(Difficulty.Hard);
                Assert.DoesNotContain(game.Obstacles, o => o.Y == 12);
            }
        }

        [Fact]
        public void Obstacles_AreUnique_AndInsideBorder()
        {
            var game = NewGame(Difficulty.Hard);

            Assert.Equal(game.Obstacles.Count, game.Obstacles.Distinct().Count());
            foreach (PixelPoint o in game.Obstacles)
            {
                Assert.InRange(o.X, 1, GameConfig.GridSize - 2);
                Assert.InRange(o.Y, 1, GameConfig.GridSize - 2);
            }
        }

        [Fact]
        public void Food_NeverSpawnsOnSnakeOrObstacle()
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                var game = NewGame(Difficulty.Hard);
                Assert.DoesNotContain(game.Food, game.Snake);
                Assert.DoesNotContain(game.Food, game.Obstacles);
            }
        }

        // ── Punktacja ───────────────────────────────────────────────────────

        [Fact]
        public void Step_SlowFood_GivesFivePoints()
        {
            var game = NewGame();
            PixelPoint head = game.Snake[0];
            game.PlaceFood(new PixelPoint(head.X + 1, head.Y), FoodType.Slow);

            Assert.Equal(StepResult.Ate, game.Step());
            Assert.Equal(5, game.Score);
            Assert.Equal(FoodType.Slow, game.LastEaten);
        }

        [Fact]
        public void Score_AccumulatesAcrossMeals()
        {
            var game = NewGame();

            for (int i = 0; i < 3; i++)
            {
                PixelPoint head = game.Snake[0];
                game.PlaceFood(new PixelPoint(head.X + 1, head.Y), FoodType.Normal);
                Assert.Equal(StepResult.Ate, game.Step());
            }

            Assert.Equal(30, game.Score);
            Assert.Equal(7, game.Snake.Count);
        }

        [Fact]
        public void Reset_ClearsScoreAndLength()
        {
            var game = NewGame();
            PixelPoint head = game.Snake[0];
            game.PlaceFood(new PixelPoint(head.X + 1, head.Y), FoodType.Bonus);
            game.Step();
            Assert.Equal(30, game.Score);

            game.Reset(DifficultySettings.For(Difficulty.Normal));

            Assert.Equal(0, game.Score);
            Assert.Equal(4, game.Snake.Count);
            Assert.Equal(Direction.Right, game.CurrentDirection);
        }

        // ── Kierunek ────────────────────────────────────────────────────────

        [Fact]
        public void SetDirection_LastValidChangeWithinOneTickWins()
        {
            var game = NewGame();
            ParkFood(game);

            game.SetDirection(Direction.Up);
            game.SetDirection(Direction.Down);   // też legalny względem Right
            game.Step();

            Assert.Equal(Direction.Down, game.CurrentDirection);
        }

        [Fact]
        public void SetDirection_ReverseIsIgnored_InEveryOrientation()
        {
            var game = NewGame();
            ParkFood(game);

            game.SetDirection(Direction.Up);
            game.Step();
            Assert.Equal(Direction.Up, game.CurrentDirection);

            game.SetDirection(Direction.Down);   // zawrócenie o 180°
            game.Step();

            Assert.Equal(Direction.Up, game.CurrentDirection);
        }
    }
}
