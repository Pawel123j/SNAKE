using Avalonia.Controls;

namespace ElegantSnake
{
    /// <summary>Okno główne — hostuje <see cref="GameControl"/>.</summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var screen = Screens.Primary;
            if (screen is not null)
            {
                Width = System.Math.Min(660, screen.WorkingArea.Width / screen.Scaling * 0.9);
                Height = System.Math.Min(738, screen.WorkingArea.Height / screen.Scaling * 0.9);
            }
        }
    }
}
