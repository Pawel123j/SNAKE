using System;
using System.IO;

namespace ElegantSnake
{
    /// <summary>
    /// Trwałe przechowywanie najlepszego wyniku w katalogu danych aplikacji,
    /// dzięki czemu rekord przeżywa zamknięcie gry.
    /// </summary>
    public static class HighScoreStore
    {
        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ElegantSnake");

        private static readonly string FilePath = Path.Combine(DirectoryPath, "highscore.txt");

        /// <summary>Wczytuje zapisany rekord; w razie problemów zwraca 0.</summary>
        public static int Load()
        {
            try
            {
                if (File.Exists(FilePath) &&
                    int.TryParse(File.ReadAllText(FilePath), out int value))
                {
                    return Math.Max(0, value);
                }
            }
            catch
            {
                // Brak dostępu do dysku nie powinien psuć gry.
            }

            return 0;
        }

        /// <summary>Zapisuje rekord; ewentualne błędy zapisu są ignorowane.</summary>
        public static void Save(int score)
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                File.WriteAllText(FilePath, score.ToString());
            }
            catch
            {
                // Wynik to miły dodatek — błąd zapisu nie może wywrócić gry.
            }
        }
    }
}
