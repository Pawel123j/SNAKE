using Avalonia;
using Xunit;

namespace ElegantSnake.Tests
{
    /// <summary>Testy czystej logiki gry (klasa <see cref="SnakeGame"/>).</summary>
    public class SnakeGameTests
    {
        private static SnakeGame NewGame(Difficulty difficulty = Difficulty.Normal)
            => new SnakeGame(DifficultySettings.For(difficulty));

        [Fact]
        public void Reset_CreatesSnakeOfFour_AndZeroScore()
        {
            var game = NewGame();

            Assert.Equal(4, game.Snake.Count);
            Assert.Equal(0, game.Score);
        }

        [Fact]
        public void Step_MovesHeadInCurrentDirection()
        {
            var game = NewGame();
            game.PlaceFood(new PixelPoint(0, 0), FoodType.Normal); // owoc poza torem węża
            PixelPoint head = game.Snake[0];

            StepResult result = game.Step();

            Assert.Equal(StepResult.Moved, result);
            Assert.Equal(new PixelPoint(head.X + 1, head.Y), game.Snake[0]);
            Assert.Equal(4, game.Snake.Count);
        }

        [Fact]
        public void SetDirection_IgnoresReverse()
        {
            var game = NewGame();
            game.PlaceFood(new PixelPoint(0, 0), FoodType.Normal);
            PixelPoint head = game.Snake[0];

            game.SetDirection(Direction.Left); // przeciwny do Right — powinien zostać zignorowany
            game.Step();

            Assert.Equal(head.X + 1, game.Snake[0].X); // wąż nadal idzie w prawo
        }

        [Fact]
        public void Step_GrowsAndScores_OnNormalFood()
        {
            var game = NewGame();
            PixelPoint head = game.Snake[0];
            game.PlaceFood(new PixelPoint(head.X + 1, head.Y), FoodType.Normal);

            StepResult result = game.Step();

            Assert.Equal(StepResult.Ate, result);
            Assert.Equal(10, game.Score);
            Assert.Equal(5, game.Snake.Count);
        }

        [Fact]
        public void Step_BonusFood_GivesThirtyPoints()
        {
            var game = NewGame();
            PixelPoint head = game.Snake[0];
            game.PlaceFood(new PixelPoint(head.X + 1, head.Y), FoodType.Bonus);

            game.Step();

            Assert.Equal(30, game.Score);
            Assert.Equal(FoodType.Bonus, game.LastEaten);
        }

        [Fact]
        public void Step_HittingWall_Dies_WhenNoWrap()
        {
            var game = NewGame(Difficulty.Normal);
            game.PlaceFood(new PixelPoint(0, 0), FoodType.Normal);

            StepResult result = StepResult.Moved;
            for (int i = 0; i < GameConfig.GridSize + 5 && result != StepResult.Died; i++)
                result = game.Step();

            Assert.Equal(StepResult.Died, result);
        }

        [Fact]
        public void Step_WrapsAround_OnEasy_WithoutDying()
        {
            var game = NewGame(Difficulty.Easy);
            game.PlaceFood(new PixelPoint(0, 0), FoodType.Normal); // wiersz 0; wąż jedzie w wierszu 12

            for (int i = 0; i < GameConfig.GridSize + 5; i++)
                Assert.NotEqual(StepResult.Died, game.Step());

            Assert.InRange(game.Snake[0].X, 0, GameConfig.GridSize - 1);
        }

        [Fact]
        public void Hard_HasObstacles()
        {
            var game = NewGame(Difficulty.Hard);
            Assert.NotEmpty(game.Obstacles);
        }

        [Fact]
        public void Normal_HasNoObstacles()
        {
            var game = NewGame(Difficulty.Normal);
            Assert.Empty(game.Obstacles);
        }
    }
}
