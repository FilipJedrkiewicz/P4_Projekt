using System.Collections.ObjectModel;
using System.Windows;
using EwidencjaPojazdow.Data;
using EwidencjaPojazdow.Models;

namespace EwidencjaPojazdow.ViewModels;

public class RejestracjeViewModel : BaseViewModel
{
    private readonly RejestracjeRepository _repo = new();
    private List<RejestracjaPojazdu> _wszystkie = [];
    public ObservableCollection<RejestracjaPojazdu> Rekordy { get; } = [];

    private string _numerRej = "";
    public string NumerRej { get => _numerRej; set => Set(ref _numerRej, value); }

    private DateTime? _dataRejestracji;
    public DateTime? DataRejestracji { get => _dataRejestracji; set => Set(ref _dataRejestracji, value); }

    private string _status = "";
    public string Status { get => _status; set => Set(ref _status, value); }

    private string _pesel = "";
    public string PESEL { get => _pesel; set => Set(ref _pesel, value); }

    private string _numerVIN = "";
    public string NumerVIN { get => _numerVIN; set => Set(ref _numerVIN, value); }

    private bool _numerRejEnabled = true;
    public bool NumerRejEnabled { get => _numerRejEnabled; set => Set(ref _numerRejEnabled, value); }

    private string _statusBar = "Gotowy";
    public string StatusBar { get => _statusBar; set => Set(ref _statusBar, value); }

    private bool _trybEdycji = false;

    private RejestracjaPojazdu? _wybrany;
    public RejestracjaPojazdu? Wybrany
    {
        get => _wybrany;
        set { Set(ref _wybrany, value); if (value != null) WypelnijFormularz(value); }
    }

    public RelayCommand DodajCommand   { get; }
    public RelayCommand ZapiszCommand  { get; }
    public RelayCommand UsunCommand    { get; }
    public RelayCommand WyczyscCommand { get; }
    public RelayCommand OdswiezCommand { get; }

    public RejestracjeViewModel()
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
            foreach (var r in _wszystkie) Rekordy.Add(r);
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
            : _wszystkie.FindAll(r =>
                r.NumerRej.ToLower().Contains(q) || r.Status.ToLower().Contains(q) ||
                r.PESEL.Contains(q) || r.NumerVIN.ToLower().Contains(q));
        Rekordy.Clear();
        foreach (var r in filtered) Rekordy.Add(r);
    }

    private void WypelnijFormularz(RejestracjaPojazdu r)
    {
        NumerRej        = r.NumerRej;
        DataRejestracji = r.DataRejestracji;
        Status          = r.Status;
        PESEL           = r.PESEL;
        NumerVIN        = r.NumerVIN;
        NumerRejEnabled = false;
        _trybEdycji     = true;
    }

    private void Dodaj() { Wyczysc(); NumerRejEnabled = true; _trybEdycji = false; }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(NumerRej))
        { MessageBox.Show("Nr rejestracyjny jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (DataRejestracji == null)
        { MessageBox.Show("Wybierz datę rejestracji.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(Status))
        { MessageBox.Show("Wybierz status.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (PESEL.Length != 11)
        { MessageBox.Show("PESEL musi mieć 11 cyfr.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (NumerVIN.Length != 17)
        { MessageBox.Show("VIN musi mieć 17 znaków.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        try
        {
            var r = new RejestracjaPojazdu
            {
                NumerRej        = NumerRej.Trim(),
                DataRejestracji = DataRejestracji.Value,
                Status          = Status,
                PESEL           = PESEL.Trim(),
                NumerVIN        = NumerVIN.Trim().ToUpper()
            };

            if (_trybEdycji) { _repo.Aktualizuj(r); StatusBar = "Zaktualizowano."; }
            else             { _repo.Dodaj(r);       StatusBar = "Dodano rejestrację."; }

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
        if (MessageBox.Show($"Usunąć rejestrację {Wybrany.NumerRej}?",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            _repo.Usun(Wybrany.NumerRej);
            StatusBar = "Usunięto rejestrację."; Wyczysc(); Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd usuwania:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Wyczysc()
    {
        NumerRej = ""; DataRejestracji = null; Status = ""; PESEL = ""; NumerVIN = "";
        NumerRejEnabled = true; _trybEdycji = false;
        _wybrany = null; OnPropertyChanged(nameof(Wybrany));
    }
}
