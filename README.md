# CEPiK (Ewidencja Pojazdów)

Aplikacja WPF do zarządzania ewidencją pojazdów wzorowana na systemie CEPiK. 

## Architektura projektu (Refaktoryzacja)
Projekt został zrefaktoryzowany z podejścia "SQL w Widoku" (code-behind) do pełnego rozdzielenia warstw przy użyciu wzorca **MVVM**:
- **Models**: Definicje encji biznesowych (kierowca, pojazd, polisa, badanie itd.).
- **Data (Repository)**: Warstwa dostępu do danych (klasy repozytoriów, w których znajduje się całe zapytania SQL).
- **ViewModels**: Klasy zarządzające logiką biznesową, wiązaniem danych i komunikatami UI, dziedziczące po `BaseViewModel`.
- **Views**: UI w plikach XAML. Pliki code-behind (`*.xaml.cs`) zostały maksymalnie uproszczone i nie zawierają żadnego kodu bazodanowego.

## Wymagania
- .NET 10.0 SDK
- MS SQL Server (lub LocalDB / SQL Server Express)
- Visual Studio 2022 (zalecana wersja z obsługą rozwiązań `.slnx`)

## Uruchomienie bazy danych
1. Uruchom skrypt `TworzenieBazy.sql` w celu utworzenia bazy `EwidencjaPojazdowDB`.
2. Opcjonalnie uruchom `wsady_testy.sql`, aby dodać dane testowe oraz widok raportu.

## Konfiguracja połączenia
Konfiguracja bazy znajduje się w pliku `cepik.cfg` umieszczonym w katalogu projektu (i kopiowanym do folderu wyjściowego kompilacji):

```json
{
  "Server": "localhost",
  "Database": "EwidencjaPojazdowDB",
  "IntegratedSecurity": true,
  "UserId": "",
  "Password": ""
}
```

- Jeżeli `IntegratedSecurity` jest ustawione na `true`, aplikacja zaloguje się za pomocą Twojego konta Windows.
- Jeżeli `IntegratedSecurity` jest `false`, należy uzupełnić pola `UserId` oraz `Password`.

## Otwieranie projektu w Visual Studio
W katalogu głównym znajduje się plik **`EwidencjaPojazdow.slnx`** (nowy, lekki format pliku rozwiązania Visual Studio).
- Otwórz plik `EwidencjaPojazdow.slnx` bezpośrednio w Visual Studio 2022, aby załadować cały projekt.
