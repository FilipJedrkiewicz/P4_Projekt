using System.Collections.ObjectModel;
using System.Windows;
using EwidencjaPojazdow.Data;
using EwidencjaPojazdow.Models;

namespace EwidencjaPojazdow.ViewModels;

public class PojazydViewModel : BaseViewModel
{
    private readonly PojazydRepository _repo = new();
    private List<Pojazd> _wszystkie = [];
    public ObservableCollection<Pojazd> Rekordy { get; } = [];

    private string _numerVIN = "";
    public string NumerVIN { get => _numerVIN; set => Set(ref _numerVIN, value); }

    private string _marka = "";
    public string Marka { get => _marka; set => Set(ref _marka, value); }

    private string _model = "";
    public string Model { get => _model; set => Set(ref _model, value); }

    private string _rokProdukcji = "";
    public string RokProdukcji { get => _rokProdukcji; set => Set(ref _rokProdukcji, value); }

    private bool _vinEnabled = true;
    public bool VINEnabled { get => _vinEnabled; set => Set(ref _vinEnabled, value); }

    private string _statusBar = "Gotowy";
    public string StatusBar { get => _statusBar; set => Set(ref _statusBar, value); }

    private bool _trybEdycji = false;

    private Pojazd? _wybrany;
    public Pojazd? Wybrany
    {
        get => _wybrany;
        set { Set(ref _wybrany, value); if (value != null) WypelnijFormularz(value); }
    }

    public RelayCommand DodajCommand   { get; }
    public RelayCommand ZapiszCommand  { get; }
    public RelayCommand UsunCommand    { get; }
    public RelayCommand WyczyscCommand { get; }
    public RelayCommand OdswiezCommand { get; }

    public PojazydViewModel()
    {
        DodajCommand   = new RelayCommand(Dodaj);
        ZapiszCommand  = new RelayCommand(Zapisz);
        UsunCommand    = new RelayCommand(Usun,   () => Wybrany != null);
        WyczyscCommand = new RelayCommand(Wyczysc);
        OdswiezCommand = new RelayCommand(Odswiez);
        Odswiez();
    }

    public void Odswiez()
    {
        try
        {
            _wszystkie = _repo.GetAll();
            Rekordy.Clear();
            foreach (var p in _wszystkie) Rekordy.Add(p);
            StatusBar = $"Załadowano {Rekordy.Count} rekordów.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd ładowania danych:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void Szukaj(string query)
    {
        var q = query.ToLower();
        var filtered = string.IsNullOrWhiteSpace(q)
            ? _wszystkie
            : _wszystkie.FindAll(p =>
                p.NumerVIN.ToLower().Contains(q) || p.Marka.ToLower().Contains(q) ||
                p.Model.ToLower().Contains(q) || p.RokProdukcji.ToString().Contains(q));
        Rekordy.Clear();
        foreach (var p in filtered) Rekordy.Add(p);
    }

    private void WypelnijFormularz(Pojazd p)
    {
        NumerVIN     = p.NumerVIN;
        Marka        = p.Marka;
        Model        = p.Model;
        RokProdukcji = p.RokProdukcji.ToString();
        VINEnabled   = false;
        _trybEdycji  = true;
    }

    private void Dodaj() { Wyczysc(); VINEnabled = true; _trybEdycji = false; }

    private void Zapisz()
    {
        if (NumerVIN.Length != 17)
        { MessageBox.Show("VIN musi mieć dokładnie 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(Marka) || string.IsNullOrWhiteSpace(Model))
        { MessageBox.Show("Marka i Model są wymagane.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (!int.TryParse(RokProdukcji, out int rok) || rok < 1900 || rok > DateTime.Now.Year)
        { MessageBox.Show($"Rok produkcji musi być liczbą między 1900 a {DateTime.Now.Year}.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        try
        {
            var p = new Pojazd
            {
                NumerVIN     = NumerVIN.Trim().ToUpper(),
                Marka        = Marka.Trim(),
                Model        = Model.Trim(),
                RokProdukcji = rok
            };

            if (_trybEdycji) { _repo.Aktualizuj(p); StatusBar = "Zaktualizowano."; }
            else             { _repo.Dodaj(p);       StatusBar = "Dodano pojazd."; }

            Wyczysc(); Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd zapisu:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Usun()
    {
        if (Wybrany == null) return;
        if (MessageBox.Show($"Usunąć pojazd {Wybrany.Marka} {Wybrany.Model} (VIN: {Wybrany.NumerVIN})?\nUsunięcie skasuje też badania i polisy.",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            _repo.Usun(Wybrany.NumerVIN);
            StatusBar = "Usunięto pojazd."; Wyczysc(); Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd usuwania:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Wyczysc()
    {
        NumerVIN = ""; Marka = ""; Model = ""; RokProdukcji = "";
        VINEnabled = true; _trybEdycji = false;
        _wybrany = null; OnPropertyChanged(nameof(Wybrany));
    }
}
