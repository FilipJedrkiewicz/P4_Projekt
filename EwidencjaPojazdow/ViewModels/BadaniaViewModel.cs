using System.Collections.ObjectModel;
using System.Windows;
using EwidencjaPojazdow.Data;
using EwidencjaPojazdow.Models;

namespace EwidencjaPojazdow.ViewModels;

public class BadaniaViewModel : BaseViewModel
{
    private readonly BadaniaRepository _repo = new();
    private List<BadanieTechniczne> _wszystkie = [];
    public ObservableCollection<BadanieTechniczne> Rekordy { get; } = [];

    private string _nrZaswiadczenia = "";
    public string NrZaswiadczenia { get => _nrZaswiadczenia; set => Set(ref _nrZaswiadczenia, value); }

    private DateTime? _dataBadania;
    public DateTime? DataBadania { get => _dataBadania; set => Set(ref _dataBadania, value); }

    private string _wynik = "";
    public string Wynik { get => _wynik; set => Set(ref _wynik, value); }

    private string _numerVIN = "";
    public string NumerVIN { get => _numerVIN; set => Set(ref _numerVIN, value); }

    private bool _nrZaswiadczeniaEnabled = true;
    public bool NrZaswiadczeniaEnabled { get => _nrZaswiadczeniaEnabled; set => Set(ref _nrZaswiadczeniaEnabled, value); }

    private string _statusBar = "Gotowy";
    public string StatusBar { get => _statusBar; set => Set(ref _statusBar, value); }

    private bool _trybEdycji = false;

    private BadanieTechniczne? _wybrany;
    public BadanieTechniczne? Wybrany
    {
        get => _wybrany;
        set { Set(ref _wybrany, value); if (value != null) WypelnijFormularz(value); }
    }

    public RelayCommand DodajCommand   { get; }
    public RelayCommand ZapiszCommand  { get; }
    public RelayCommand UsunCommand    { get; }
    public RelayCommand WyczyscCommand { get; }
    public RelayCommand OdswiezCommand { get; }

    public BadaniaViewModel()
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
            foreach (var b in _wszystkie) Rekordy.Add(b);
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
            : _wszystkie.FindAll(b =>
                b.NrZaswiadczenia.ToLower().Contains(q) || b.Wynik.ToLower().Contains(q) ||
                b.NumerVIN.ToLower().Contains(q));
        Rekordy.Clear();
        foreach (var b in filtered) Rekordy.Add(b);
    }

    private void WypelnijFormularz(BadanieTechniczne b)
    {
        NrZaswiadczenia         = b.NrZaswiadczenia;
        DataBadania             = b.DataBadania;
        Wynik                   = b.Wynik;
        NumerVIN                = b.NumerVIN;
        NrZaswiadczeniaEnabled  = false;
        _trybEdycji             = true;
    }

    private void Dodaj() { Wyczysc(); NrZaswiadczeniaEnabled = true; _trybEdycji = false; }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(NrZaswiadczenia))
        { MessageBox.Show("Nr zaświadczenia jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (DataBadania == null)
        { MessageBox.Show("Wybierz datę badania.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(Wynik))
        { MessageBox.Show("Wybierz wynik.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (NumerVIN.Length != 17)
        { MessageBox.Show("VIN musi mieć 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        try
        {
            var b = new BadanieTechniczne
            {
                NrZaswiadczenia = NrZaswiadczenia.Trim(),
                DataBadania     = DataBadania.Value,
                Wynik           = Wynik,
                NumerVIN        = NumerVIN.Trim().ToUpper()
            };

            if (_trybEdycji) { _repo.Aktualizuj(b); StatusBar = "Zaktualizowano."; }
            else             { _repo.Dodaj(b);       StatusBar = "Dodano badanie."; }

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
        if (MessageBox.Show($"Usunąć badanie {Wybrany.NrZaswiadczenia}?",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            _repo.Usun(Wybrany.NrZaswiadczenia);
            StatusBar = "Usunięto badanie."; Wyczysc(); Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd usuwania:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Wyczysc()
    {
        NrZaswiadczenia = ""; DataBadania = null; Wynik = ""; NumerVIN = "";
        NrZaswiadczeniaEnabled = true; _trybEdycji = false;
        _wybrany = null; OnPropertyChanged(nameof(Wybrany));
    }
}
