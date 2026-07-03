using System;

namespace EwidencjaPojazdow.Models
{
    public class Kierowca
    {
        public string PESEL { get; set; } = "";
        public string Imie { get; set; } = "";
        public string Nazwisko { get; set; } = "";
        public string Miejscowosc { get; set; } = "";
        public DateTime DataUrodzenia { get; set; }

        public string ImieNazwisko => $"{Imie} {Nazwisko}";
    }

    public class PrawoJazdy
    {
        public string NumerDruku { get; set; } = "";
        public string Kategorie { get; set; } = "";
        public DateTime DataWydania { get; set; }
        public string PESEL { get; set; } = "";
    }

    public class Pojazd
    {
        public string NumerVIN { get; set; } = "";
        public string Marka { get; set; } = "";
        public string Model { get; set; } = "";
        public int RokProdukcji { get; set; }
    }

    public class RejestracjaPojazdu
    {
        public string NumerRej { get; set; } = "";
        public DateTime DataRejestracji { get; set; }
        public string Status { get; set; } = "";
        public string PESEL { get; set; } = "";
        public string NumerVIN { get; set; } = "";
    }

    public class BadanieTechniczne
    {
        public string NrZaswiadczenia { get; set; } = "";
        public DateTime DataBadania { get; set; }
        public string Wynik { get; set; } = "";
        public string NumerVIN { get; set; } = "";
    }

    public class PolisaOC
    {
        public string NumerPolisy { get; set; } = "";
        public string Ubezpieczyciel { get; set; } = "";
        public DateTime DataWaznosci { get; set; }
        public string NumerVIN { get; set; } = "";
    }
}
