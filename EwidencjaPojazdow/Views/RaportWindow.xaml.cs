using System.Data;
using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.ViewModels;

namespace EwidencjaPojazdow.Views;

public partial class RaportWindow : Window
{
    private readonly RaportViewModel _vm;

    public RaportWindow()
    {
        InitializeComponent();
        _vm = new RaportViewModel();
        DataContext = _vm;
        // Podpinamy DataView do DataGrid po załadowaniu danych
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_vm.Dane))
                DgRaport.ItemsSource = _vm.Dane?.DefaultView;
        };
        DgRaport.ItemsSource = _vm.Dane?.DefaultView;
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_vm.Dane == null) return;
        var q = TxtSearch.Text.Trim();
        if (string.IsNullOrWhiteSpace(q))
        {
            _vm.Dane.DefaultView.RowFilter = "";
        }
        else
        {
            var filters = new List<string>();
            foreach (DataColumn col in _vm.Dane.Columns)
                filters.Add($"CONVERT([{col.ColumnName}], System.String) LIKE '%{q}%'");
            _vm.Dane.DefaultView.RowFilter = string.Join(" OR ", filters);
        }
    }

    private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => _vm.OdswiezCommand.Execute(null);
}
