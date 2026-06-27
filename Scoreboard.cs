using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ElegantSnake
{
    /// <summary>
    /// Tabela najlepszych wyników (top 5) zapisywana w katalogu danych aplikacji,
    /// dzięki czemu rekordy przeżywają zamknięcie gry.
    /// </summary>
    public sealed class Scoreboard
    {
        public const int MaxEntries = 5;

        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ElegantSnake");

        private static readonly string FilePath = Path.Combine(DirectoryPath, "scores.txt");

        private readonly List<int> scores = new List<int>();

        /// <summary>Wyniki posortowane malejąco (najlepszy pierwszy).</summary>
        public IReadOnlyList<int> Scores => scores;

        /// <summary>Najlepszy wynik lub 0, gdy brak.</summary>
        public int Best => scores.Count > 0 ? scores[0] : 0;

        /// <summary>Wczytuje zapisane wyniki; w razie problemów zostaje pusta lista.</summary>
        public void Load()
        {
            scores.Clear();
            try
            {
                if (File.Exists(FilePath))
                {
                    foreach (string line in File.ReadAllLines(FilePath))
                        if (int.TryParse(line, out int value) && value > 0)
                            scores.Add(value);

                    Normalize();
                }
            }
            catch
            {
                // Brak dostępu do dysku nie powinien psuć gry.
            }
        }

        /// <summary>Dodaje wynik i zapisuje. Zwraca true, jeśli trafił do najlepszej piątki.</summary>
        public bool Add(int score)
        {
            if (score <= 0)
                return false;

            bool qualifies = scores.Count < MaxEntries || score > scores[scores.Count - 1];

            scores.Add(score);
            Normalize();
            Save();

            return qualifies;
        }

        private void Normalize()
        {
            scores.Sort((a, b) => b.CompareTo(a));
            if (scores.Count > MaxEntries)
                scores.RemoveRange(MaxEntries, scores.Count - MaxEntries);
        }

        private void Save()
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                File.WriteAllLines(FilePath, scores.Select(s => s.ToString()));
            }
            catch
            {
                // Zapis to tylko miły dodatek — błąd nie może wywrócić gry.
            }
        }
    }
}
