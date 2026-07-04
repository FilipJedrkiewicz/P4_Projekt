using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Views
{
    public partial class PolisyWindow : Window
    {
        private List<PolisaOC> _wszystkie = new();
        private bool _trybEdycji = false;

        public PolisyWindow() { InitializeComponent(); ZaladujDane(); }

        private void ZaladujDane()
        {
            try
            {
                _wszystkie = new();
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                using var reader = new SqlCommand("SELECT Numer_polisy, Ubezpieczyciel, Data_waznosci, Numer_VIN FROM Polisa_OC ORDER BY Data_waznosci DESC", conn).ExecuteReader();
                while (reader.Read())
                    _wszystkie.Add(new PolisaOC { NumerPolisy = reader.GetString(0), Ubezpieczyciel = reader.GetString(1), DataWaznosci = reader.GetDateTime(2), NumerVIN = reader.GetString(3) });
                
                DgPolisy.ItemsSource = null;
                DgPolisy.ItemsSource = _wszystkie;
                StatusBar.Text = $"Załadowano {_wszystkie.Count} rekordów.";
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            DgPolisy.ItemsSource = string.IsNullOrWhiteSpace(q) ? _wszystkie
                : _wszystkie.FindAll(p => p.NumerPolisy.ToLower().Contains(q) || p.Ubezpieczyciel.ToLower().Contains(q) || p.NumerVIN.ToLower().Contains(q));
        }

        private void Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPolisy.SelectedItem is PolisaOC p)
            {
                TxtNumerPolisy.Text = p.NumerPolisy; TxtUbezpieczyciel.Text = p.Ubezpieczyciel;
                DpDataWaznosci.SelectedDate = p.DataWaznosci; TxtVIN.Text = p.NumerVIN;
                TxtNumerPolisy.IsEnabled = false; _trybEdycji = true;
            }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e) { WyczyscFormularz(); TxtNumerPolisy.IsEnabled = true; }
        private void BtnEdytuj_Click(object sender, RoutedEventArgs e) { if (DgPolisy.SelectedItem == null) MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => ZaladujDane();
        private void BtnWyczysc_Click(object sender, RoutedEventArgs e) => WyczyscFormularz();

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            if (DgPolisy.SelectedItem is not PolisaOC p) { MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            if (MessageBox.Show($"Usunąć polisę {p.NumerPolisy}?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var cmd = new SqlCommand("DELETE FROM Polisa_OC WHERE Numer_polisy=@N", conn);
                cmd.Parameters.AddWithValue("@N", p.NumerPolisy); cmd.ExecuteNonQuery();
                StatusBar.Text = "Usunięto polisę."; WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNumerPolisy.Text)) { MessageBox.Show("Nr polisy jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (string.IsNullOrWhiteSpace(TxtUbezpieczyciel.Text)) { MessageBox.Show("Ubezpieczyciel jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (DpDataWaznosci.SelectedDate == null) { MessageBox.Show("Wybierz datę ważności.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (TxtVIN.Text.Length != 17) { MessageBox.Show("VIN musi mieć 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                SqlCommand cmd;
                if (_trybEdycji)
                {
                    cmd = new SqlCommand("UPDATE Polisa_OC SET Ubezpieczyciel=@U, Data_waznosci=@D, Numer_VIN=@V WHERE Numer_polisy=@N", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNumerPolisy.Text.Trim());
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO Polisa_OC (Numer_polisy,Ubezpieczyciel,Data_waznosci,Numer_VIN) VALUES(@N,@U,@D,@V)", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNumerPolisy.Text.Trim());
                }
                cmd.Parameters.AddWithValue("@U", TxtUbezpieczyciel.Text.Trim());
                cmd.Parameters.AddWithValue("@D", DpDataWaznosci.SelectedDate!.Value);
                cmd.Parameters.AddWithValue("@V", TxtVIN.Text.Trim().ToUpper());
                cmd.ExecuteNonQuery();
                StatusBar.Text = _trybEdycji ? "Zaktualizowano." : "Dodano polisę.";
                WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd zapisu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void WyczyscFormularz()
        {
            TxtNumerPolisy.Text = ""; TxtUbezpieczyciel.Text = ""; DpDataWaznosci.SelectedDate = null;
            TxtVIN.Text = ""; TxtNumerPolisy.IsEnabled = true; _trybEdycji = false; DgPolisy.SelectedItem = null;
        }
    }
}
