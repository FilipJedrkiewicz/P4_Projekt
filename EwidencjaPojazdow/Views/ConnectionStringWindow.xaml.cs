using System.Windows;
using EwidencjaPojazdow.Database;

namespace EwidencjaPojazdow.Views
{
    public partial class ConnectionStringWindow : Window
    {
        public string ConnectionString { get; private set; } = "";

        public ConnectionStringWindow()
        {
            InitializeComponent();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectionString = $"Server={TxtServer.Text};Database={TxtDatabase.Text};" +
                               "Integrated Security=True;TrustServerCertificate=True;";
            
            // Zapisz konfigurację do pliku cepik.cfg, aby nie wpisywać jej ponownie
            DatabaseHelper.SaveConfig(TxtServer.Text, TxtDatabase.Text);
            
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
