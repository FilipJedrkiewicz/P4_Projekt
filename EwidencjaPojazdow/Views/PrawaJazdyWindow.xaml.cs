using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Views
{
    public partial class PrawaJazdyWindow : Window
    {
        private List<PrawoJazdy> _wszystkie = new();
        private bool _trybEdycji = false;

        public PrawaJazdyWindow() { InitializeComponent(); ZaladujDane(); }

        private void ZaladujDane()
        {
            try
            {
                _wszystkie = new();
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var reader = new SqlCommand("SELECT Numer_druku, Kategorie, Data_wydania, PESEL FROM Prawo_Jazdy ORDER BY Numer_druku", conn).ExecuteReader();
                while (reader.Read())
                    _wszystkie.Add(new PrawoJazdy { NumerDruku = reader.GetString(0), Kategorie = reader.GetString(1), DataWydania = reader.GetDateTime(2), PESEL = reader.GetString(3) });
                
                DgPrawaJazdy.ItemsSource = null;
                DgPrawaJazdy.ItemsSource = _wszystkie;
                StatusBar.Text = $"Załadowano {_wszystkie.Count} rekordów.";
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            DgPrawaJazdy.ItemsSource = string.IsNullOrWhiteSpace(q) ? _wszystkie
                : _wszystkie.FindAll(p => p.NumerDruku.ToLower().Contains(q) || p.PESEL.Contains(q) || p.Kategorie.ToLower().Contains(q));
        }

        private void Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPrawaJazdy.SelectedItem is PrawoJazdy p)
            {
                TxtNumerDruku.Text = p.NumerDruku; TxtKategorie.Text = p.Kategorie;
                DpDataWydania.SelectedDate = p.DataWydania; TxtPESEL.Text = p.PESEL;
                TxtNumerDruku.IsEnabled = false; _trybEdycji = true;
            }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e) { WyczyscFormularz(); TxtNumerDruku.IsEnabled = true; }
        private void BtnEdytuj_Click(object sender, RoutedEventArgs e) { if (DgPrawaJazdy.SelectedItem == null) MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => ZaladujDane();
        private void BtnWyczysc_Click(object sender, RoutedEventArgs e) => WyczyscFormularz();

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            if (DgPrawaJazdy.SelectedItem is not PrawoJazdy p) { MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            if (MessageBox.Show($"Usunąć prawo jazdy {p.NumerDruku}?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var cmd = new SqlCommand("DELETE FROM Prawo_Jazdy WHERE Numer_druku = @N", conn);
                cmd.Parameters.AddWithValue("@N", p.NumerDruku); cmd.ExecuteNonQuery();
                StatusBar.Text = "Usunięto rekord."; WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNumerDruku.Text)) { MessageBox.Show("Numer druku jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (DpDataWydania.SelectedDate == null) { MessageBox.Show("Wybierz datę wydania.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (TxtPESEL.Text.Length != 11) { MessageBox.Show("PESEL musi mieć 11 cyfr.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                SqlCommand cmd;
                if (_trybEdycji)
                {
                    cmd = new SqlCommand("UPDATE Prawo_Jazdy SET Kategorie=@K, Data_wydania=@D, PESEL=@P WHERE Numer_druku=@N", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNumerDruku.Text.Trim());
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO Prawo_Jazdy (Numer_druku,Kategorie,Data_wydania,PESEL) VALUES(@N,@K,@D,@P)", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNumerDruku.Text.Trim());
                }
                cmd.Parameters.AddWithValue("@K", TxtKategorie.Text.Trim());
                cmd.Parameters.AddWithValue("@D", DpDataWydania.SelectedDate!.Value);
                cmd.Parameters.AddWithValue("@P", TxtPESEL.Text.Trim());
                cmd.ExecuteNonQuery();
                StatusBar.Text = _trybEdycji ? "Zaktualizowano." : "Dodano nowy rekord.";
                WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd zapisu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void WyczyscFormularz()
        {
            TxtNumerDruku.Text = ""; TxtKategorie.Text = ""; DpDataWydania.SelectedDate = null;
            TxtPESEL.Text = ""; TxtNumerDruku.IsEnabled = true; _trybEdycji = false; DgPrawaJazdy.SelectedItem = null;
        }
    }
}
