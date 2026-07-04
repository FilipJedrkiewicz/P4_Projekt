using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Views
{
    public partial class PojazydWindow : Window
    {
        private List<Pojazd> _wszystkie = new();
        private bool _trybEdycji = false;

        public PojazydWindow() { InitializeComponent(); ZaladujDane(); }

        private void ZaladujDane()
        {
            try
            {
                _wszystkie = new();
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                using var reader = new SqlCommand("SELECT Numer_VIN, Marka, Model, Rok_produkcji FROM Pojazd ORDER BY Marka, Model", conn).ExecuteReader();
                while (reader.Read())
                    _wszystkie.Add(new Pojazd { NumerVIN = reader.GetString(0), Marka = reader.GetString(1), Model = reader.GetString(2), RokProdukcji = reader.GetInt32(3) });
                
                DgPojazdy.ItemsSource = null;
                DgPojazdy.ItemsSource = _wszystkie;
                StatusBar.Text = $"Załadowano {_wszystkie.Count} rekordów.";
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            DgPojazdy.ItemsSource = string.IsNullOrWhiteSpace(q) ? _wszystkie
                : _wszystkie.FindAll(p => p.NumerVIN.ToLower().Contains(q) || p.Marka.ToLower().Contains(q) || p.Model.ToLower().Contains(q) || p.RokProdukcji.ToString().Contains(q));
        }

        private void Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPojazdy.SelectedItem is Pojazd p)
            {
                TxtVIN.Text = p.NumerVIN; TxtMarka.Text = p.Marka; TxtModel.Text = p.Model; TxtRok.Text = p.RokProdukcji.ToString();
                TxtVIN.IsEnabled = false; _trybEdycji = true;
            }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e) { WyczyscFormularz(); TxtVIN.IsEnabled = true; }
        private void BtnEdytuj_Click(object sender, RoutedEventArgs e) { if (DgPojazdy.SelectedItem == null) MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => ZaladujDane();
        private void BtnWyczysc_Click(object sender, RoutedEventArgs e) => WyczyscFormularz();

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            if (DgPojazdy.SelectedItem is not Pojazd p) { MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            if (MessageBox.Show($"Usunąć pojazd {p.Marka} {p.Model} (VIN: {p.NumerVIN})?\nUsunięcie skasuje też badania i polisy.", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var cmd = new SqlCommand("DELETE FROM Pojazd WHERE Numer_VIN = @V", conn);
                cmd.Parameters.AddWithValue("@V", p.NumerVIN); cmd.ExecuteNonQuery();
                StatusBar.Text = "Usunięto pojazd."; WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            if (TxtVIN.Text.Length != 17) { MessageBox.Show("VIN musi mieć dokładnie 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (string.IsNullOrWhiteSpace(TxtMarka.Text) || string.IsNullOrWhiteSpace(TxtModel.Text)) { MessageBox.Show("Marka i Model są wymagane.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!int.TryParse(TxtRok.Text, out int rok) || rok < 1900 || rok > DateTime.Now.Year) { MessageBox.Show($"Rok produkcji musi być liczbą między 1900 a {DateTime.Now.Year}.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                SqlCommand cmd;
                if (_trybEdycji)
                {
                    cmd = new SqlCommand("UPDATE Pojazd SET Marka=@M, Model=@Mo, Rok_produkcji=@R WHERE Numer_VIN=@V", conn);
                    cmd.Parameters.AddWithValue("@V", TxtVIN.Text.Trim());
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO Pojazd (Numer_VIN,Marka,Model,Rok_produkcji) VALUES(@V,@M,@Mo,@R)", conn);
                    cmd.Parameters.AddWithValue("@V", TxtVIN.Text.Trim().ToUpper());
                }
                cmd.Parameters.AddWithValue("@M", TxtMarka.Text.Trim());
                cmd.Parameters.AddWithValue("@Mo", TxtModel.Text.Trim());
                cmd.Parameters.AddWithValue("@R", rok);
                cmd.ExecuteNonQuery();
                StatusBar.Text = _trybEdycji ? "Zaktualizowano." : "Dodano pojazd.";
                WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd zapisu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void WyczyscFormularz()
        {
            TxtVIN.Text = ""; TxtMarka.Text = ""; TxtModel.Text = ""; TxtRok.Text = "";
            TxtVIN.IsEnabled = true; _trybEdycji = false; DgPojazdy.SelectedItem = null;
        }
    }
}
