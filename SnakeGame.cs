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

        /// <summary>Wąż zginął (ściana, przeszkoda lub własne ciało).</summary>
        Died
    }

    /// <summary>
    /// Czysta logika gry — bez rysowania. Odpowiada za węża, jedzenie,
    /// przeszkody, kolizje i wynik. Zachowanie (tempo, owijanie ścian, liczba
    /// przeszkód) zależy od przekazanych <see cref="DifficultySettings"/>.
    /// </summary>
    public sealed class SnakeGame
    {
        private readonly Random random = new Random();
        private readonly List<PixelPoint> snake = new List<PixelPoint>();
        private readonly List<PixelPoint> obstacles = new List<PixelPoint>();
        private Direction nextDirection;
        private DifficultySettings settings;

        public SnakeGame(DifficultySettings settings)
        {
            this.settings = settings;
            Reset(settings);
        }

        /// <summary>Segmenty węża — głowa jest pod indeksem 0.</summary>
        public IReadOnlyList<PixelPoint> Snake => snake;

        /// <summary>Stałe przeszkody na planszy.</summary>
        public IReadOnlyList<PixelPoint> Obstacles => obstacles;

        /// <summary>Pozycja owocu (we współrzędnych siatki).</summary>
        public PixelPoint Food { get; private set; }

        /// <summary>Rodzaj aktualnie leżącego owocu.</summary>
        public FoodType FoodKind { get; private set; }

        /// <summary>Rodzaj ostatnio zjedzonego owocu (do efektów po stronie UI).</summary>
        public FoodType LastEaten { get; private set; }

        /// <summary>Kierunek, w którym wąż faktycznie się porusza.</summary>
        public Direction CurrentDirection { get; private set; }

        /// <summary>Bieżący wynik.</summary>
        public int Score { get; private set; }

        /// <summary>Aktualne ustawienia poziomu trudności.</summary>
        public DifficultySettings Settings => settings;

        /// <summary>Ustawia grę do stanu początkowego dla danego poziomu.</summary>
        public void Reset(DifficultySettings newSettings)
        {
            settings = newSettings;

            snake.Clear();
            snake.Add(new PixelPoint(8, 12));
            snake.Add(new PixelPoint(7, 12));
            snake.Add(new PixelPoint(6, 12));
            snake.Add(new PixelPoint(5, 12));

            CurrentDirection = Direction.Right;
            nextDirection = Direction.Right;
            Score = 0;

            BuildObstacles();
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
            int nx = head.X;
            int ny = head.Y;

            switch (CurrentDirection)
            {
                case Direction.Up: ny--; break;
                case Direction.Down: ny++; break;
                case Direction.Left: nx--; break;
                default: nx++; break;
            }

            if (settings.WrapAround)
            {
                nx = (nx + GameConfig.GridSize) % GameConfig.GridSize;
                ny = (ny + GameConfig.GridSize) % GameConfig.GridSize;
            }
            else if (nx < 0 || nx >= GameConfig.GridSize || ny < 0 || ny >= GameConfig.GridSize)
            {
                return StepResult.Died;
            }

            var newHead = new PixelPoint(nx, ny);

            if (obstacles.Contains(newHead))
                return StepResult.Died;

            bool grows = newHead == Food;
            IEnumerable<PixelPoint> bodyToCheck = grows ? snake : snake.Take(snake.Count - 1);
            if (bodyToCheck.Contains(newHead))
                return StepResult.Died;

            snake.Insert(0, newHead);

            if (grows)
            {
                LastEaten = FoodKind;
                Score += PointsFor(FoodKind);
                SpawnFood();
                return StepResult.Ate;
            }

            snake.RemoveAt(snake.Count - 1);
            return StepResult.Moved;
        }

        private static int PointsFor(FoodType type) => type switch
        {
            FoodType.Bonus => 30,
            FoodType.Slow => 5,
            _ => 10
        };

        private void BuildObstacles()
        {
            obstacles.Clear();

            int guard = 0;
            while (obstacles.Count < settings.ObstacleCount && guard++ < 2000)
            {
                var p = new PixelPoint(
                    random.Next(1, GameConfig.GridSize - 1),
                    random.Next(1, GameConfig.GridSize - 1));

                if (p.Y == 12) continue;             // nie blokuj startowego rzędu węża
                if (snake.Contains(p)) continue;
                if (obstacles.Contains(p)) continue;

                obstacles.Add(p);
            }
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
            while (snake.Contains(candidate) || obstacles.Contains(candidate));

            Food = candidate;
            FoodKind = RollFoodType();
        }

        private FoodType RollFoodType()
        {
            double r = random.NextDouble();
            if (r < 0.15) return FoodType.Bonus;
            if (r < 0.30) return FoodType.Slow;
            return FoodType.Normal;
        }

        /// <summary>
        /// Ustawia owoc deterministycznie — wyłącznie na potrzeby testów jednostkowych.
        /// </summary>
        internal void PlaceFood(PixelPoint position, FoodType type)
        {
            Food = position;
            FoodKind = type;
        }
    }
}
