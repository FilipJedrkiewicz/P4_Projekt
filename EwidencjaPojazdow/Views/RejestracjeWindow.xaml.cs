using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Views
{
    public partial class RejestracjeWindow : Window
    {
        private List<RejestracjaPojazdu> _wszystkie = new();
        private bool _trybEdycji = false;

        public RejestracjeWindow() { InitializeComponent(); ZaladujDane(); }

        private void ZaladujDane()
        {
            try
            {
                _wszystkie = new();
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                using var reader = new SqlCommand("SELECT Numer_rej, Data_rejestracji, Status, PESEL, Numer_VIN FROM Rejestracja_Pojazdu ORDER BY Data_rejestracji DESC", conn).ExecuteReader();
                while (reader.Read())
                    _wszystkie.Add(new RejestracjaPojazdu { NumerRej = reader.GetString(0), DataRejestracji = reader.GetDateTime(1), Status = reader.GetString(2), PESEL = reader.GetString(3), NumerVIN = reader.GetString(4) });
                
                DgRejestracje.ItemsSource = null;
                DgRejestracje.ItemsSource = _wszystkie;
                StatusBar.Text = $"Załadowano {_wszystkie.Count} rekordów.";
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            DgRejestracje.ItemsSource = string.IsNullOrWhiteSpace(q) ? _wszystkie
                : _wszystkie.FindAll(r => r.NumerRej.ToLower().Contains(q) || r.Status.ToLower().Contains(q) || r.PESEL.Contains(q) || r.NumerVIN.ToLower().Contains(q));
        }

        private void Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgRejestracje.SelectedItem is RejestracjaPojazdu r)
            {
                TxtNumerRej.Text = r.NumerRej; DpData.SelectedDate = r.DataRejestracji;
                TxtPESEL.Text = r.PESEL; TxtVIN.Text = r.NumerVIN;
                foreach (ComboBoxItem item in CbStatus.Items)
                    if (item.Content.ToString() == r.Status) { CbStatus.SelectedItem = item; break; }
                TxtNumerRej.IsEnabled = false; _trybEdycji = true;
            }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e) { WyczyscFormularz(); TxtNumerRej.IsEnabled = true; }
        private void BtnEdytuj_Click(object sender, RoutedEventArgs e) { if (DgRejestracje.SelectedItem == null) MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => ZaladujDane();
        private void BtnWyczysc_Click(object sender, RoutedEventArgs e) => WyczyscFormularz();

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            if (DgRejestracje.SelectedItem is not RejestracjaPojazdu r) { MessageBox.Show("Wybierz rekord.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            if (MessageBox.Show($"Usunąć rejestrację {r.NumerRej}?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var cmd = new SqlCommand("DELETE FROM Rejestracja_Pojazdu WHERE Numer_rej=@N", conn);
                cmd.Parameters.AddWithValue("@N", r.NumerRej); cmd.ExecuteNonQuery();
                StatusBar.Text = "Usunięto rejestrację."; WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNumerRej.Text)) { MessageBox.Show("Nr rejestracyjny jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (DpData.SelectedDate == null) { MessageBox.Show("Wybierz datę rejestracji.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (CbStatus.SelectedItem == null) { MessageBox.Show("Wybierz status.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (TxtPESEL.Text.Length != 11) { MessageBox.Show("PESEL musi mieć 11 cyfr.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (TxtVIN.Text.Length != 17) { MessageBox.Show("VIN musi mieć 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var status = ((ComboBoxItem)CbStatus.SelectedItem).Content.ToString()!;
                SqlCommand cmd;
                if (_trybEdycji)
                {
                    cmd = new SqlCommand("UPDATE Rejestracja_Pojazdu SET Data_rejestracji=@D, Status=@S, PESEL=@P, Numer_VIN=@V WHERE Numer_rej=@N", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNumerRej.Text.Trim());
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO Rejestracja_Pojazdu (Numer_rej,Data_rejestracji,Status,PESEL,Numer_VIN) VALUES(@N,@D,@S,@P,@V)", conn);
                    cmd.Parameters.AddWithValue("@N", TxtNumerRej.Text.Trim());
                }
                cmd.Parameters.AddWithValue("@D", DpData.SelectedDate!.Value);
                cmd.Parameters.AddWithValue("@S", status);
                cmd.Parameters.AddWithValue("@P", TxtPESEL.Text.Trim());
                cmd.Parameters.AddWithValue("@V", TxtVIN.Text.Trim().ToUpper());
                cmd.ExecuteNonQuery();
                StatusBar.Text = _trybEdycji ? "Zaktualizowano." : "Dodano rejestrację.";
                WyczyscFormularz(); ZaladujDane();
            }
            catch (Exception ex) { MessageBox.Show($"Błąd zapisu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void WyczyscFormularz()
        {
            TxtNumerRej.Text = ""; DpData.SelectedDate = null; CbStatus.SelectedItem = null;
            TxtPESEL.Text = ""; TxtVIN.Text = "";
            TxtNumerRej.IsEnabled = true; _trybEdycji = false; DgRejestracje.SelectedItem = null;
        }
    }
}
