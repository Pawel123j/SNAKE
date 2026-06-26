using System;
using Avalonia;

namespace ElegantSnake
{
    /// <summary>Punkt wejścia aplikacji Avalonia.</summary>
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        /// <summary>Konfiguracja Avalonii (używana też przez narzędzia projektowe).</summary>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
