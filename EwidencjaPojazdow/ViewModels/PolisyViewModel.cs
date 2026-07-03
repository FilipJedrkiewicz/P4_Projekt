using System.Collections.ObjectModel;
using System.Windows;
using EwidencjaPojazdow.Data;
using EwidencjaPojazdow.Models;

namespace EwidencjaPojazdow.ViewModels;

public class PolisyViewModel : BaseViewModel
{
    private readonly PolisyRepository _repo = new();
    private List<PolisaOC> _wszystkie = [];
    public ObservableCollection<PolisaOC> Rekordy { get; } = [];

    private string _numerPolisy = "";
    public string NumerPolisy { get => _numerPolisy; set => Set(ref _numerPolisy, value); }

    private string _ubezpieczyciel = "";
    public string Ubezpieczyciel { get => _ubezpieczyciel; set => Set(ref _ubezpieczyciel, value); }

    private DateTime? _dataWaznosci;
    public DateTime? DataWaznosci { get => _dataWaznosci; set => Set(ref _dataWaznosci, value); }

    private string _numerVIN = "";
    public string NumerVIN { get => _numerVIN; set => Set(ref _numerVIN, value); }

    private bool _numerPolisyEnabled = true;
    public bool NumerPolisyEnabled { get => _numerPolisyEnabled; set => Set(ref _numerPolisyEnabled, value); }

    private string _statusBar = "Gotowy";
    public string StatusBar { get => _statusBar; set => Set(ref _statusBar, value); }

    private bool _trybEdycji = false;

    private PolisaOC? _wybrany;
    public PolisaOC? Wybrany
    {
        get => _wybrany;
        set { Set(ref _wybrany, value); if (value != null) WypelnijFormularz(value); }
    }

    public RelayCommand DodajCommand   { get; }
    public RelayCommand ZapiszCommand  { get; }
    public RelayCommand UsunCommand    { get; }
    public RelayCommand WyczyscCommand { get; }
    public RelayCommand OdswiezCommand { get; }

    public PolisyViewModel()
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
                p.NumerPolisy.ToLower().Contains(q) || p.Ubezpieczyciel.ToLower().Contains(q) ||
                p.NumerVIN.ToLower().Contains(q));
        Rekordy.Clear();
        foreach (var p in filtered) Rekordy.Add(p);
    }

    private void WypelnijFormularz(PolisaOC p)
    {
        NumerPolisy        = p.NumerPolisy;
        Ubezpieczyciel     = p.Ubezpieczyciel;
        DataWaznosci       = p.DataWaznosci;
        NumerVIN           = p.NumerVIN;
        NumerPolisyEnabled = false;
        _trybEdycji        = true;
    }

    private void Dodaj() { Wyczysc(); NumerPolisyEnabled = true; _trybEdycji = false; }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(NumerPolisy))
        { MessageBox.Show("Nr polisy jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(Ubezpieczyciel))
        { MessageBox.Show("Ubezpieczyciel jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (DataWaznosci == null)
        { MessageBox.Show("Wybierz datę ważności.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (NumerVIN.Length != 17)
        { MessageBox.Show("VIN musi mieć 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        try
        {
            var p = new PolisaOC
            {
                NumerPolisy    = NumerPolisy.Trim(),
                Ubezpieczyciel = Ubezpieczyciel.Trim(),
                DataWaznosci   = DataWaznosci.Value,
                NumerVIN       = NumerVIN.Trim().ToUpper()
            };

            if (_trybEdycji) { _repo.Aktualizuj(p); StatusBar = "Zaktualizowano."; }
            else             { _repo.Dodaj(p);       StatusBar = "Dodano polisę."; }

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
        if (MessageBox.Show($"Usunąć polisę {Wybrany.NumerPolisy}?",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            _repo.Usun(Wybrany.NumerPolisy);
            StatusBar = "Usunięto polisę."; Wyczysc(); Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd usuwania:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Wyczysc()
    {
        NumerPolisy = ""; Ubezpieczyciel = ""; DataWaznosci = null; NumerVIN = "";
        NumerPolisyEnabled = true; _trybEdycji = false;
        _wybrany = null; OnPropertyChanged(nameof(Wybrany));
    }
}
