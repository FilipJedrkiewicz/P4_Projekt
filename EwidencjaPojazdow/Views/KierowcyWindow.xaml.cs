using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Views
{
    public partial class KierowcyWindow : Window
    {
        private List<Kierowca> _wszystkieRekordy = new();
        private bool _trybEdycji = false;

        public KierowcyWindow()
        {
            InitializeComponent();
            ZaladujDane();
        }

        private void ZaladujDane()
        {
            try
            {
                _wszystkieRekordy = new List<Kierowca>();
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new SqlCommand("SELECT PESEL, Imie, Nazwisko, Miejscowosc, Data_urodzenia FROM Kierowca_Wlasciciel ORDER BY Nazwisko, Imie", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _wszystkieRekordy.Add(new Kierowca
                    {
                        PESEL = reader.GetString(0),
                        Imie = reader.GetString(1),
                        Nazwisko = reader.GetString(2),
                        Miejscowosc = reader.GetString(3),
                        DataUrodzenia = reader.GetDateTime(4)
                    });
                }
                DgKierowcy.ItemsSource = null;
                DgKierowcy.ItemsSource = _wszystkieRekordy;
                StatusBar.Text = $"Załadowano {_wszystkieRekordy.Count} rekordów.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania danych:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(q))
                DgKierowcy.ItemsSource = _wszystkieRekordy;
            else
                DgKierowcy.ItemsSource = _wszystkieRekordy.FindAll(k =>
                    k.PESEL.Contains(q) || k.Imie.ToLower().Contains(q) ||
                    k.Nazwisko.ToLower().Contains(q) || k.Miejscowosc.ToLower().Contains(q));
        }

        private void DgKierowcy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgKierowcy.SelectedItem is Kierowca k)
            {
                TxtPESEL.Text = k.PESEL;
                TxtImie.Text = k.Imie;
                TxtNazwisko.Text = k.Nazwisko;
                TxtMiejscowosc.Text = k.Miejscowosc;
                DpDataUrodzenia.SelectedDate = k.DataUrodzenia;
                TxtPESEL.IsEnabled = false;
                _trybEdycji = true;
            }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e)
        {
            WyczyscFormularz();
            TxtPESEL.IsEnabled = true;
            _trybEdycji = false;
            TxtPESEL.Focus();
        }

        private void BtnEdytuj_Click(object sender, RoutedEventArgs e)
        {
            if (DgKierowcy.SelectedItem == null)
                MessageBox.Show("Wybierz rekord z listy.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            if (DgKierowcy.SelectedItem is not Kierowca k)
            {
                MessageBox.Show("Wybierz rekord do usunięcia.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var confirm = MessageBox.Show($"Usunąć kierowcę {k.ImieNazwisko} ({k.PESEL})?\nUsunięcie skasuje też powiązane prawa jazdy.", 
                "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Kierowca_Wlasciciel WHERE PESEL = @PESEL", conn);
                cmd.Parameters.AddWithValue("@PESEL", k.PESEL);
                cmd.ExecuteNonQuery();
                StatusBar.Text = $"Usunięto: {k.ImieNazwisko}";
                WyczyscFormularz();
                ZaladujDane();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd usuwania:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtPESEL.Text) || TxtPESEL.Text.Length != 11)
            {
                MessageBox.Show("PESEL musi mieć dokładnie 11 cyfr.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtImie.Text) || string.IsNullOrWhiteSpace(TxtNazwisko.Text))
            {
                MessageBox.Show("Imię i nazwisko są wymagane.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DpDataUrodzenia.SelectedDate == null)
            {
                MessageBox.Show("Wybierz datę urodzenia.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                if (_trybEdycji)
                {
                    var cmd = new SqlCommand(@"UPDATE Kierowca_Wlasciciel 
                        SET Imie=@Imie, Nazwisko=@Nazwisko, Miejscowosc=@Miej, Data_urodzenia=@Data
                        WHERE PESEL=@PESEL", conn);
                    cmd.Parameters.AddWithValue("@Imie", TxtImie.Text.Trim());
                    cmd.Parameters.AddWithValue("@Nazwisko", TxtNazwisko.Text.Trim());
                    cmd.Parameters.AddWithValue("@Miej", TxtMiejscowosc.Text.Trim());
                    cmd.Parameters.AddWithValue("@Data", DpDataUrodzenia.SelectedDate!.Value);
                    cmd.Parameters.AddWithValue("@PESEL", TxtPESEL.Text.Trim());
                    cmd.ExecuteNonQuery();
                    StatusBar.Text = "Zaktualizowano rekord.";
                }
                else
                {
                    var cmd = new SqlCommand(@"INSERT INTO Kierowca_Wlasciciel (PESEL, Imie, Nazwisko, Miejscowosc, Data_urodzenia)
                        VALUES (@PESEL, @Imie, @Nazwisko, @Miej, @Data)", conn);
                    cmd.Parameters.AddWithValue("@PESEL", TxtPESEL.Text.Trim());
                    cmd.Parameters.AddWithValue("@Imie", TxtImie.Text.Trim());
                    cmd.Parameters.AddWithValue("@Nazwisko", TxtNazwisko.Text.Trim());
                    cmd.Parameters.AddWithValue("@Miej", TxtMiejscowosc.Text.Trim());
                    cmd.Parameters.AddWithValue("@Data", DpDataUrodzenia.SelectedDate!.Value);
                    cmd.ExecuteNonQuery();
                    StatusBar.Text = "Dodano nowy rekord.";
                }

                WyczyscFormularz();
                ZaladujDane();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnWyczysc_Click(object sender, RoutedEventArgs e) => WyczyscFormularz();
        private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => ZaladujDane();

        private void WyczyscFormularz()
        {
            TxtPESEL.Text = "";
            TxtImie.Text = "";
            TxtNazwisko.Text = "";
            TxtMiejscowosc.Text = "";
            DpDataUrodzenia.SelectedDate = null;
            TxtPESEL.IsEnabled = true;
            _trybEdycji = false;
            DgKierowcy.SelectedItem = null;
        }
    }
}
