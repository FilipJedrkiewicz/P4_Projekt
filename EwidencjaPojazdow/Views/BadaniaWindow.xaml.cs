using System.Windows;
using System.Windows.Controls;
using EwidencjaPojazdow.ViewModels;

namespace EwidencjaPojazdow.Views;

public partial class BadaniaWindow : Window
{
    private readonly BadaniaViewModel _vm;

    public BadaniaWindow()
    {
        InitializeComponent();
        _vm = new BadaniaViewModel();
        DataContext = _vm;
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        => _vm.Szukaj(TxtSearch.Text);

    private void Dg_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void BtnDodaj_Click(object sender, RoutedEventArgs e)    => _vm.DodajCommand.Execute(null);
    private void BtnEdytuj_Click(object sender, RoutedEventArgs e)   { }
    private void BtnUsun_Click(object sender, RoutedEventArgs e)     => _vm.UsunCommand.Execute(null);
    private void BtnZapisz_Click(object sender, RoutedEventArgs e)   => _vm.ZapiszCommand.Execute(null);
    private void BtnWyczysc_Click(object sender, RoutedEventArgs e)  => _vm.WyczyscCommand.Execute(null);
    private void BtnOdswierz_Click(object sender, RoutedEventArgs e) => _vm.OdswiezCommand.Execute(null);
}
