using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Views
{
    public partial class BadaniaWindow : Window
    {
        private List<BadanieTechniczne> _wszystkie = new();
        private bool _trybEdycji = false;

        public BadaniaWindow() { InitializeComponent(); ZaladujDane(); }

        private void ZaladujDane()
        {
            try
            {
                _wszystkie = new();
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                using var reader = new SqlCommand("SELECT Nr_zaswiadczenia, Data_badania, Wynik, Numer_VIN FROM Badanie_Techniczne ORDER BY Data_badania DESC", conn).ExecuteReader();
                while (reader.Read())
                    _wszystkie.Add(new BadanieTechniczne { NrZaswiadczenia = reader.GetString(0), DataBadania = reader.GetDateTime(1), Wynik = reader.GetString(2), NumerVIN = reader.GetString(3) });
                
                DgBadania.ItemsSource = null;
                DgBadania.ItemsSource = _wszystkie;
                StatusBar.Text = $"Załadowano {_wszystkie.Count} rekordów.";
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            DgBadania.ItemsSource = string.IsNullOrWhiteSpace(q) ? _wszystkie
                : _wszystkie.FindAll(b => b.NrZaswiadczenia.ToLower().Contains(q) || b.Wynik.ToLower().Contains(q) || b.NumerVIN.ToLower().Contains(q));
        }

        private void Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgBadania.SelectedItem is BadanieTechniczne b)
            {
                TxtNrZasw.Text = b.NrZaswiadczenia; DpData.SelectedDate = b.DataBadania; TxtVIN.Text = b.NumerVIN;
                foreach (ComboBoxItem item in CbWynik.Items)
                    if (item.Content.ToString() == b.Wynik) { CbWynik.SelectedItem = item; break; }
                TxtNrZasw.IsEnabled = false; _trybEdycji = true;
            }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e) { WyczyscFormularz(); TxtNrZasw.IsEnabled = true; }
        private void BtnEdytuj_Click(object sender, RoutedEventArgs e) { if (DgBadania.SelectedItem == null) MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => ZaladujDane();
        private void BtnWyczysc_Click(object sender, RoutedEventArgs e) => WyczyscFormularz();

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            if (DgBadania.SelectedItem is not BadanieTechniczne b) { MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            if (MessageBox.Show($"Usunąć badanie {b.NrZaswiadczenia}?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var cmd = new SqlCommand("DELETE FROM Badanie_Techniczne WHERE Nr_zaswiadczenia=@N", conn);
                cmd.Parameters.AddWithValue("@N", b.NrZaswiadczenia); cmd.ExecuteNonQuery();
                StatusBar.Text = "Usunięto badanie."; WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNrZasw.Text)) { MessageBox.Show("Nr zaświadczenia jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (DpData.SelectedDate == null) { MessageBox.Show("Wybierz datę badania.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (CbWynik.SelectedItem == null) { MessageBox.Show("Wybierz wynik.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (TxtVIN.Text.Length != 17) { MessageBox.Show("VIN musi mieć 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var wynik = ((ComboBoxItem)CbWynik.SelectedItem).Content.ToString()!;
                SqlCommand cmd;
                if (_trybEdycji)
                {
                    cmd = new SqlCommand("UPDATE Badanie_Techniczne SET Data_badania=@D, Wynik=@W, Numer_VIN=@V WHERE Nr_zaswiadczenia=@N", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNrZasw.Text.Trim());
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO Badanie_Techniczne (Nr_zaswiadczenia,Data_badania,Wynik,Numer_VIN) VALUES(@N,@D,@W,@V)", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNrZasw.Text.Trim());
                }
                cmd.Parameters.AddWithValue("@D", DpData.SelectedDate!.Value);
                cmd.Parameters.AddWithValue("@W", wynik);
                cmd.Parameters.AddWithValue("@V", TxtVIN.Text.Trim().ToUpper());
                cmd.ExecuteNonQuery();
                StatusBar.Text = _trybEdycji ? "Zaktualizowano." : "Dodano badanie.";
                WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd zapisu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void WyczyscFormularz()
        {
            TxtNrZasw.Text = ""; DpData.SelectedDate = null; CbWynik.SelectedItem = null;
            TxtVIN.Text = ""; TxtNrZasw.IsEnabled = true; _trybEdycji = false; DgBadania.SelectedItem = null;
        }
    }
}
