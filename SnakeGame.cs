using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;

namespace ElegantSnake
{
    /// <summary>Wynik pojedynczego kroku symulacji gry.</summary>
    public enum StepResult
    {
        /// <summary>Wąż przesunął się bez zdarzeń.</summary>
        Moved,

        /// <summary>Wąż zjadł owoc i urósł.</summary>
        Ate,

        /// <summary>Wąż zginął (ściana lub własne ciało).</summary>
        Died
    }

    /// <summary>
    /// Czysta logika gry — bez rysowania. Odpowiada za węża, jedzenie,
    /// kolizje i wynik. Współrzędne komórek trzymamy w <see cref="PixelPoint"/>
    /// (lekki, całkowitoliczbowy typ Avalonii).
    /// </summary>
    public sealed class SnakeGame
    {
        private readonly Random random = new Random();
        private readonly List<PixelPoint> snake = new List<PixelPoint>();
        private Direction nextDirection;

        public SnakeGame()
        {
            Reset();
        }

        /// <summary>Segmenty węża — głowa jest pod indeksem 0.</summary>
        public IReadOnlyList<PixelPoint> Snake => snake;

        /// <summary>Aktualna pozycja owocu (we współrzędnych siatki).</summary>
        public PixelPoint Food { get; private set; }

        /// <summary>Kierunek, w którym wąż faktycznie się porusza.</summary>
        public Direction CurrentDirection { get; private set; }

        /// <summary>Bieżący wynik.</summary>
        public int Score { get; private set; }

        /// <summary>Ustawia grę do stanu początkowego.</summary>
        public void Reset()
        {
            snake.Clear();
            snake.Add(new PixelPoint(8, 12));
            snake.Add(new PixelPoint(7, 12));
            snake.Add(new PixelPoint(6, 12));
            snake.Add(new PixelPoint(5, 12));

            CurrentDirection = Direction.Right;
            nextDirection = Direction.Right;
            Score = 0;
            SpawnFood();
        }

        /// <summary>
        /// Zgłasza chęć zmiany kierunku. Zawrócenie o 180° jest ignorowane,
        /// bo wąż wpadłby sam na siebie.
        /// </summary>
        public void SetDirection(Direction direction)
        {
            bool isOpposite =
                (direction == Direction.Up && CurrentDirection == Direction.Down) ||
                (direction == Direction.Down && CurrentDirection == Direction.Up) ||
                (direction == Direction.Left && CurrentDirection == Direction.Right) ||
                (direction == Direction.Right && CurrentDirection == Direction.Left);

            if (!isOpposite)
                nextDirection = direction;
        }

        /// <summary>
        /// Wykonuje jeden krok symulacji: przesuwa węża i rozwiązuje kolizje.
        /// </summary>
        public StepResult Step()
        {
            CurrentDirection = nextDirection;

            PixelPoint head = snake[0];
            PixelPoint newHead = CurrentDirection switch
            {
                Direction.Up => new PixelPoint(head.X, head.Y - 1),
                Direction.Down => new PixelPoint(head.X, head.Y + 1),
                Direction.Left => new PixelPoint(head.X - 1, head.Y),
                _ => new PixelPoint(head.X + 1, head.Y)
            };

            if (newHead.X < 0 || newHead.X >= GameConfig.GridSize ||
                newHead.Y < 0 || newHead.Y >= GameConfig.GridSize)
            {
                return StepResult.Died;
            }

            bool grows = newHead == Food;
            IEnumerable<PixelPoint> bodyToCheck = grows ? snake : snake.Take(snake.Count - 1);
            if (bodyToCheck.Contains(newHead))
                return StepResult.Died;

            snake.Insert(0, newHead);

            if (grows)
            {
                Score += GameConfig.PointsPerFood;
                SpawnFood();
                return StepResult.Ate;
            }

            snake.RemoveAt(snake.Count - 1);
            return StepResult.Moved;
        }

        private void SpawnFood()
        {
            PixelPoint candidate;
            do
            {
                candidate = new PixelPoint(
                    random.Next(0, GameConfig.GridSize),
                    random.Next(0, GameConfig.GridSize));
            }
            while (snake.Contains(candidate));

            Food = candidate;
        }
    }
}
