using System.Collections.ObjectModel;
using System.Windows;
using EwidencjaPojazdow.Data;
using EwidencjaPojazdow.Models;

namespace EwidencjaPojazdow.ViewModels;

public class PrawaJazdyViewModel : BaseViewModel
{
    private readonly PrawaJazdyRepository _repo = new();
    private List<PrawoJazdy> _wszystkie = [];
    public ObservableCollection<PrawoJazdy> Rekordy { get; } = [];

    private string _numerDruku = "";
    public string NumerDruku { get => _numerDruku; set => Set(ref _numerDruku, value); }

    private string _kategorie = "";
    public string Kategorie { get => _kategorie; set => Set(ref _kategorie, value); }

    private DateTime? _dataWydania;
    public DateTime? DataWydania { get => _dataWydania; set => Set(ref _dataWydania, value); }

    private string _pesel = "";
    public string PESEL { get => _pesel; set => Set(ref _pesel, value); }

    private bool _numerDrukuEnabled = true;
    public bool NumerDrukuEnabled { get => _numerDrukuEnabled; set => Set(ref _numerDrukuEnabled, value); }

    private string _statusBar = "Gotowy";
    public string StatusBar { get => _statusBar; set => Set(ref _statusBar, value); }

    private bool _trybEdycji = false;

    private PrawoJazdy? _wybrany;
    public PrawoJazdy? Wybrany
    {
        get => _wybrany;
        set { Set(ref _wybrany, value); if (value != null) WypelnijFormularz(value); }
    }

    public RelayCommand DodajCommand   { get; }
    public RelayCommand ZapiszCommand  { get; }
    public RelayCommand UsunCommand    { get; }
    public RelayCommand WyczyscCommand { get; }
    public RelayCommand OdswiezCommand { get; }

    public PrawaJazdyViewModel()
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
                p.NumerDruku.ToLower().Contains(q) || p.PESEL.Contains(q) ||
                p.Kategorie.ToLower().Contains(q));
        Rekordy.Clear();
        foreach (var p in filtered) Rekordy.Add(p);
    }

    private void WypelnijFormularz(PrawoJazdy p)
    {
        NumerDruku        = p.NumerDruku;
        Kategorie         = p.Kategorie;
        DataWydania       = p.DataWydania;
        PESEL             = p.PESEL;
        NumerDrukuEnabled = false;
        _trybEdycji       = true;
    }

    private void Dodaj() { Wyczysc(); NumerDrukuEnabled = true; _trybEdycji = false; }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(NumerDruku))
        { MessageBox.Show("Numer druku jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (DataWydania == null)
        { MessageBox.Show("Wybierz datę wydania.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (PESEL.Length != 11)
        { MessageBox.Show("PESEL musi mieć 11 cyfr.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        try
        {
            var p = new PrawoJazdy
            {
                NumerDruku  = NumerDruku.Trim(),
                Kategorie   = Kategorie.Trim(),
                DataWydania = DataWydania.Value,
                PESEL       = PESEL.Trim()
            };

            if (_trybEdycji) { _repo.Aktualizuj(p); StatusBar = "Zaktualizowano rekord."; }
            else             { _repo.Dodaj(p);       StatusBar = "Dodano nowy rekord."; }

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
        if (MessageBox.Show($"Usunąć prawo jazdy {Wybrany.NumerDruku}?",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            _repo.Usun(Wybrany.NumerDruku);
            StatusBar = "Usunięto rekord."; Wyczysc(); Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd usuwania:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Wyczysc()
    {
        NumerDruku = ""; Kategorie = ""; DataWydania = null; PESEL = "";
        NumerDrukuEnabled = true; _trybEdycji = false;
        _wybrany = null; OnPropertyChanged(nameof(Wybrany));
    }
}
