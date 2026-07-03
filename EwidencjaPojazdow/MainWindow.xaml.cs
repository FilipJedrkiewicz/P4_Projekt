using System.Windows;
using System.Windows.Media;
using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Views;

namespace EwidencjaPojazdow
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckConnection();
        }

        private void CheckConnection()
        {
            if (DatabaseHelper.TestConnection())
            {
                StatusDot.Fill = new SolidColorBrush(Colors.LightGreen);
                StatusText.Text = "Połączono z bazą danych";
                StatusBar.Text = "Połączono z EwidencjaPojazdowDB";
            }
            else
            {
                StatusDot.Fill = new SolidColorBrush(Colors.Red);
                StatusText.Text = "Brak połączenia z bazą";
                StatusBar.Text = "BŁĄD: Brak połączenia z SQL Server. Sprawdź connection string.";

                var result = MessageBox.Show(
                    "Nie można połączyć się z bazą danych.\n\n" +
                    "Domyślny serwer: localhost\n" +
                    "Baza: EwidencjaPojazdowDB\n\n" +
                    "Czy chcesz wprowadzić własne dane połączenia?",
                    "Błąd połączenia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var connDialog = new ConnectionStringWindow();
                    if (connDialog.ShowDialog() == true)
                    {
                        DatabaseHelper.ConnectionString = connDialog.ConnectionString;
                        CheckConnection();
                    }
                }
            }
        }

        private void BtnKierowcy_Click(object sender, RoutedEventArgs e)
        {
            new KierowcyWindow().Show();
        }

        private void BtnPrawaJazdy_Click(object sender, RoutedEventArgs e)
        {
            new PrawaJazdyWindow().Show();
        }

        private void BtnPojazdy_Click(object sender, RoutedEventArgs e)
        {
            new PojazydWindow().Show();
        }

        private void BtnRejestracje_Click(object sender, RoutedEventArgs e)
        {
            new RejestracjeWindow().Show();
        }

        private void BtnBadania_Click(object sender, RoutedEventArgs e)
        {
            new BadaniaWindow().Show();
        }

        private void BtnPolisy_Click(object sender, RoutedEventArgs e)
        {
            new PolisyWindow().Show();
        }

        private void BtnRaport_Click(object sender, RoutedEventArgs e)
        {
            new RaportWindow().Show();
        }
    }
}