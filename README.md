# CEPiK - System Ewidencji Pojazdów

Prosta aplikacja CRUD w technologii WPF (.NET) współpracująca z bazą danych MS SQL Server.

## Baza danych
Przed uruchomieniem programu wykonaj w bazie SQL skrypty (w podanej kolejności):
1. `1_StworzBazeISchemat.sql` (schemat tabel, widoki, triggery)
2. `2_DaneTestowe.sql` (uzupełnienie przykładowych danych)

## Konfiguracja
Ustawienia serwera bazy danych znajdują się w pliku `cepik.cfg`. Jeśli program nie połączy się automatycznie, przy starcie otworzy się okno, w którym można podać nazwę serwera SQL.
