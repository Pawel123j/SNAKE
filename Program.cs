using System;
using System.Windows.Forms;

namespace ElegantSnake
{
    /// <summary>
    /// Punkt wejścia aplikacji — uruchamia okno z grą.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SnakeForm());
        }
    }
}
