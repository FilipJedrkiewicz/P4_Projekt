using System.Collections.ObjectModel;
using System.Windows;
using EwidencjaPojazdow.Data;
using EwidencjaPojazdow.Models;

namespace EwidencjaPojazdow.ViewModels;

public class KierowcyViewModel : BaseViewModel
{
    private readonly KierowcyRepository _repo = new();

    // --- Lista ---
    private List<Kierowca> _wszystkie = [];
    public ObservableCollection<Kierowca> Rekordy { get; } = [];

    // --- Formularz ---
    private string _pesel = "";
    public string PESEL
    {
        get => _pesel;
        set => Set(ref _pesel, value);
    }

    private string _imie = "";
    public string Imie
    {
        get => _imie;
        set => Set(ref _imie, value);
    }

    private string _nazwisko = "";
    public string Nazwisko
    {
        get => _nazwisko;
        set => Set(ref _nazwisko, value);
    }

    private string _miejscowosc = "";
    public string Miejscowosc
    {
        get => _miejscowosc;
        set => Set(ref _miejscowosc, value);
    }

    private DateTime? _dataUrodzenia;
    public DateTime? DataUrodzenia
    {
        get => _dataUrodzenia;
        set => Set(ref _dataUrodzenia, value);
    }

    private bool _peselEnabled = true;
    public bool PeselEnabled
    {
        get => _peselEnabled;
        set => Set(ref _peselEnabled, value);
    }

    private string _statusBar = "Gotowy";
    public string StatusBar
    {
        get => _statusBar;
        set => Set(ref _statusBar, value);
    }

    // --- Tryb edycji ---
    private bool _trybEdycji = false;

    // --- Komendy ---
    public RelayCommand DodajCommand    { get; }
    public RelayCommand ZapiszCommand   { get; }
    public RelayCommand UsunCommand     { get; }
    public RelayCommand WyczyscCommand  { get; }
    public RelayCommand OdswiezCommand  { get; }

    // --- Wybrany rekord ---
    private Kierowca? _wybrany;
    public Kierowca? Wybrany
    {
        get => _wybrany;
        set
        {
            Set(ref _wybrany, value);
            if (value != null) WypelnijFormularz(value);
        }
    }

    public KierowcyViewModel()
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
            foreach (var k in _wszystkie) Rekordy.Add(k);
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
            : _wszystkie.FindAll(k =>
                k.PESEL.Contains(q) || k.Imie.ToLower().Contains(q) ||
                k.Nazwisko.ToLower().Contains(q) || k.Miejscowosc.ToLower().Contains(q));

        Rekordy.Clear();
        foreach (var k in filtered) Rekordy.Add(k);
    }

    private void WypelnijFormularz(Kierowca k)
    {
        PESEL         = k.PESEL;
        Imie          = k.Imie;
        Nazwisko      = k.Nazwisko;
        Miejscowosc   = k.Miejscowosc;
        DataUrodzenia = k.DataUrodzenia;
        PeselEnabled  = false;
        _trybEdycji   = true;
    }

    private void Dodaj()
    {
        Wyczysc();
        PeselEnabled = true;
        _trybEdycji  = false;
    }

    private void Zapisz()
    {
        if (PESEL.Length != 11)
        {
            MessageBox.Show("PESEL musi mieć dokładnie 11 cyfr.", "Walidacja",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(Imie) || string.IsNullOrWhiteSpace(Nazwisko))
        {
            MessageBox.Show("Imię i nazwisko są wymagane.", "Walidacja",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (DataUrodzenia == null)
        {
            MessageBox.Show("Wybierz datę urodzenia.", "Walidacja",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var k = new Kierowca
            {
                PESEL         = PESEL.Trim(),
                Imie          = Imie.Trim(),
                Nazwisko      = Nazwisko.Trim(),
                Miejscowosc   = Miejscowosc.Trim(),
                DataUrodzenia = DataUrodzenia.Value
            };

            if (_trybEdycji)
            {
                _repo.Aktualizuj(k);
                StatusBar = "Zaktualizowano rekord.";
            }
            else
            {
                _repo.Dodaj(k);
                StatusBar = "Dodano nowy rekord.";
            }

            Wyczysc();
            Odswiez();
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
        var confirm = MessageBox.Show(
            $"Usunąć kierowcę {Wybrany.ImieNazwisko} ({Wybrany.PESEL})?\nUsunięcie skasuje też powiązane prawa jazdy.",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            _repo.Usun(Wybrany.PESEL);
            StatusBar = $"Usunięto: {Wybrany.ImieNazwisko}";
            Wyczysc();
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd usuwania:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Wyczysc()
    {
        PESEL         = "";
        Imie          = "";
        Nazwisko      = "";
        Miejscowosc   = "";
        DataUrodzenia = null;
        PeselEnabled  = true;
        _trybEdycji   = false;
        _wybrany      = null;
        OnPropertyChanged(nameof(Wybrany));
    }
}
