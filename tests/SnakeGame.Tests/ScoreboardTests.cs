using System;
using System.IO;
using Xunit;

namespace ElegantSnake.Tests
{
    /// <summary>
    /// Testy tabeli wyników. Każdy działa we własnym katalogu tymczasowym —
    /// nigdy w prawdziwym katalogu danych aplikacji, żeby nie nadpisać
    /// rekordów gracza i nie zależeć od tego, co już tam leży.
    /// </summary>
    public sealed class ScoreboardTests : IDisposable
    {
        private readonly string workDir =
            Path.Combine(Path.GetTempPath(), "snake-scores-" + Guid.NewGuid().ToString("N"));

        private Scoreboard NewBoard() => new Scoreboard(workDir);

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(workDir))
                    Directory.Delete(workDir, recursive: true);
            }
            catch
            {
                // Sprzątanie katalogu tymczasowego nie może wywrócić testu.
            }
        }

        [Fact]
        public void Best_IsZero_WhenEmpty()
        {
            Assert.Equal(0, NewBoard().Best);
        }

        [Fact]
        public void Add_SortsDescending()
        {
            var board = NewBoard();

            board.Add(30);
            board.Add(100);
            board.Add(60);

            Assert.Equal(new[] { 100, 60, 30 }, board.Scores);
            Assert.Equal(100, board.Best);
        }

        [Fact]
        public void Add_KeepsOnlyTopFive()
        {
            var board = NewBoard();

            for (int score = 10; score <= 100; score += 10)
                board.Add(score);

            Assert.Equal(Scoreboard.MaxEntries, board.Scores.Count);
            Assert.Equal(new[] { 100, 90, 80, 70, 60 }, board.Scores);
        }

        [Fact]
        public void Add_RejectsNonPositiveScores()
        {
            var board = NewBoard();

            Assert.False(board.Add(0));
            Assert.False(board.Add(-5));
            Assert.Empty(board.Scores);
        }

        [Fact]
        public void Add_ReturnsFalse_WhenScoreDoesNotBeatTopFive()
        {
            var board = NewBoard();
            for (int score = 50; score <= 90; score += 10)
                board.Add(score);   // pełna piątka: 90, 80, 70, 60, 50

            Assert.False(board.Add(10));
            Assert.True(board.Add(95));
        }

        [Fact]
        public void Load_ReadsBackSavedScores()
        {
            var first = NewBoard();
            first.Add(40);
            first.Add(120);

            var second = NewBoard();
            second.Load();

            Assert.Equal(new[] { 120, 40 }, second.Scores);
        }

        [Fact]
        public void Load_IgnoresGarbageLines()
        {
            Directory.CreateDirectory(workDir);
            File.WriteAllLines(Path.Combine(workDir, "scores.txt"),
                new[] { "70", "to nie liczba", "", "-3", "0", "110" });

            var board = NewBoard();
            board.Load();

            // Zostają tylko wartości dodatnie, posortowane malejąco.
            Assert.Equal(new[] { 110, 70 }, board.Scores);
        }

        [Fact]
        public void Load_OnMissingFile_LeavesBoardEmpty()
        {
            var board = NewBoard();
            board.Load();

            Assert.Empty(board.Scores);
            Assert.Equal(0, board.Best);
        }
    }
}
