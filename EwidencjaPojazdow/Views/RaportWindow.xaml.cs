using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.Database;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Views
{
    public partial class RaportWindow : Window
    {
        private DataTable? _dane;

        public RaportWindow() { InitializeComponent(); ZaladujDane(); }

        private void ZaladujDane()
        {
            try
            {
                _dane = new DataTable();
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var adapter = new SqlDataAdapter("SELECT * FROM VW_ZintegrowanyRaportPojazdu", conn);
                adapter.Fill(_dane);
                DgRaport.ItemsSource = _dane.DefaultView;
                StatusBar.Text = $"Załadowano {_dane.Rows.Count} rekordów z widoku.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania raportu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_dane == null) return;
            var q = TxtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(q))
                _dane.DefaultView.RowFilter = "";
            else
            {
                var filters = new System.Collections.Generic.List<string>();
                foreach (DataColumn col in _dane.Columns)
                    filters.Add($"CONVERT([{col.ColumnName}], System.String) LIKE '%{q}%'");
                _dane.DefaultView.RowFilter = string.Join(" OR ", filters);
            }
        }

        private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => ZaladujDane();
    }
}
