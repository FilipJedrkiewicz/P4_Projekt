# System Ewidencji Pojazdów (CEPiK)

Aplikacja desktopowa typu CRUD napisana w technologii WPF (.NET) służąca do zarządzania bazą danych ewidencji pojazdów, kierowców, rejestracji, badań technicznych oraz polis ubezpieczeniowych OC. 

## Funkcjonalności
* **Zarządzanie kierowcami i prawami jazdy (CRUD)** – dodawanie, edycja i usuwanie danych osobowych oraz kategorii uprawnień.
* **Ewidencja pojazdów oraz rejestracji** – przypisywanie tablic rejestracyjnych do pojazdów i kierowców.
* **Badania techniczne** – rejestracja wpisów z automatyczną walidacją chronologii (badanie nie może być wcześniejsze niż data rejestracji pojazdu).
* **Polisy OC** – kontrola ważności ubezpieczenia pojazdów.
* **Raport zbiorczy** – wbudowany podgląd zintegrowanego raportu pojazdów na podstawie dedykowanego widoku w bazie danych.
* **Konfigurator połączenia** – automatyczne wykrywanie problemu z połączeniem przy starcie aplikacji i możliwość wprowadzenia własnego serwera SQL bezpośrednio z interfejsu użytkownika (zapisywane do pliku konfiguracyjnego `cepik.cfg`).

## Struktura bazy danych (SQL)
Wszystkie skrypty znajdują się w katalogu głównym projektu:
1. `1_StworzBazeISchemat.sql` – Tworzy bazę danych `EwidencjaPojazdowDB`, tabele, relacje, klucze, a także widok raportu, funkcję sprawdzania OC oraz trigger chronologii badań.
2. `2_DaneTestowe.sql` – Uzupełnia bazę początkowymi, przykładowymi rekordami dla ułatwienia testów.

## Wymagania i uruchomienie
1. Uruchom serwer MS SQL Server.
2. Wykonaj w programie SSMS (lub innym narzędziu bazodanowym) skrypty `1_StworzBazeISchemat.sql` oraz `2_DaneTestowe.sql` (w tej kolejności).
3. Otwórz plik rozwiązania `EwidencjaPojazdow.slnx` w Visual Studio 2022.
4. Uruchom projekt (klawisz F5).

### Konfiguracja połączenia
Ustawienia połączenia z bazą danych znajdują się w pliku `cepik.cfg` w katalogu projektu:
```json
{
  "Server": "localhost",
  "Database": "EwidencjaPojazdowDB",
  "IntegratedSecurity": true,
  "UserId": "",
  "Password": ""
}
```
Jeśli przy starcie program nie połączy się z bazą danych (np. masz inną instancję serwera SQL), otworzy się okno konfiguracji, w którym możesz podać poprawną nazwę serwera. Po kliknięciu "Połącz" nowe ustawienia zostaną automatycznie zapisane do pliku `cepik.cfg`.
