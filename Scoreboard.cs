using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ElegantSnake
{
    /// <summary>
    /// Tabela najlepszych wyników (top 5) zapisywana w katalogu danych aplikacji,
    /// dzięki czemu rekordy przeżywają zamknięcie gry.
    /// </summary>
    public sealed class Scoreboard
    {
        public const int MaxEntries = 5;

        private static readonly string DefaultDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
            "ElegantSnake");

        private readonly string directoryPath;
        private readonly string filePath;
        private readonly List<int> scores = new List<int>();

        /// <summary>Tabela wyników w standardowym katalogu danych aplikacji.</summary>
        public Scoreboard() : this(DefaultDirectoryPath)
        {
        }

        /// <summary>
        /// Tabela wyników we wskazanym katalogu.
        /// </summary>
        /// <remarks>
        /// Katalog był wcześniej zaszyty na stałe, przez co testy tej klasy
        /// pisałyby do prawdziwego pliku gracza — nadpisując jego rekordy
        /// i zależąc od tego, co już tam leży. Możliwość wskazania katalogu
        /// jest tu wyłącznie po to, żeby dało się ją przetestować w izolacji;
        /// gra używa konstruktora bezparametrowego.
        /// </remarks>
        internal Scoreboard(string directoryPath)
        {
            this.directoryPath = directoryPath;
            filePath = Path.Combine(directoryPath, "scores.txt");
        }

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
                if (File.Exists(filePath))
                {
                    foreach (string line in File.ReadAllLines(filePath))
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
            string temporary = Path.Combine(directoryPath, ".scores-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                Directory.CreateDirectory(directoryPath);
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                    {
                        foreach (int score in scores) writer.WriteLine(score);
                        writer.Flush();
                    }
                    stream.Flush(flushToDisk: true);
                }
                // Same-directory replacement keeps the previous complete file if writing fails.
                File.Move(temporary, filePath, overwrite: true);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            finally
            {
                try { File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
