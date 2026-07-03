using System.Data;
using System.Windows;
using EwidencjaPojazdow.Data;

namespace EwidencjaPojazdow.ViewModels;

public class RaportViewModel : BaseViewModel
{
    private readonly RaportRepository _repo = new();

    private DataTable? _dane;
    public DataTable? Dane
    {
        get => _dane;
        private set => Set(ref _dane, value);
    }

    private string _statusBar = "Gotowy";
    public string StatusBar { get => _statusBar; set => Set(ref _statusBar, value); }

    public RelayCommand OdswiezCommand { get; }

    public RaportViewModel()
    {
        OdswiezCommand = new RelayCommand(Odswiez);
        Odswiez();
    }

    public void Odswiez()
    {
        try
        {
            Dane = _repo.GetRaport();
            StatusBar = $"Załadowano {Dane.Rows.Count} rekordów z widoku.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd ładowania raportu:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
