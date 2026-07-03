USE EwidencjaPojazdowDB;
GO

PRINT '=== CZYSZCZENIE TABEL (ABY UNIKNĄĆ ZDUBLOWANYCH REKORDÓW) ===';
DELETE FROM Polisa_OC;
DELETE FROM Badanie_Techniczne;
DELETE FROM Rejestracja_Pojazdu;
DELETE FROM Pojazd;
DELETE FROM Prawo_Jazdy;
DELETE FROM Kierowca_Wlasciciel;
GO

PRINT '=== 1. WPROWADZANIE KIEROWCÓW / WŁAŚCICIELI ===';
INSERT INTO Kierowca_Wlasciciel (PESEL, Imie, Nazwisko, Miejscowosc, Data_urodzenia) VALUES
('95031512345', 'Jan',   'Kowalski',   'Warszawa', '1995-03-15'),
('92081054321', 'Anna',  'Nowak',      'Kraków',   '1992-08-10'),
('88112398765', 'Piotr', 'Wisniewski', 'Poznan',   '1988-11-23');
GO

PRINT '=== 2. WPROWADZANIE PRAW JAZDY ===';
INSERT INTO Prawo_Jazdy (Numer_druku, Kategorie, Data_wydania, PESEL) VALUES
('12345/05/1234', 'B', '2015-05-10', '95031512345'),
('54321/12/4321', 'B, C', '2018-12-15', '92081054321'),
('98765/20/9876', 'A, B', '2020-03-20', '88112398765');
GO

PRINT '=== 3. WPROWADZANIE POJAZDÓW ===';
INSERT INTO Pojazd (Numer_VIN, Marka, Model, Rok_produkcji) VALUES
('1FA6P8CF0H1234567', 'Ford', 'Mustang', 2017),
('WA1ABCDE2H7654321', 'Audi', 'A4', 2019),
('WBA3A51000K111111', 'BMW', 'Seria 3', 2020);
GO

PRINT '=== 4. WPROWADZANIE REJESTRACJI POJAZDÓW ===';
INSERT INTO Rejestracja_Pojazdu (Numer_rej, Data_rejestracji, Status, PESEL, Numer_VIN) VALUES
('WI12345', '2025-01-10', 'Aktywny', '95031512345', '1FA6P8CF0H1234567'),
('KR77777', '2025-03-15', 'Aktywny', '92081054321', 'WA1ABCDE2H7654321'),
('PO99999', '2026-02-20', 'Aktywny', '88112398765', 'WBA3A51000K111111');
GO

PRINT '=== 5. WPROWADZANIE BADAŃ TECHNICZNYCH ===';
INSERT INTO Badanie_Techniczne (Nr_zaswiadczenia, Data_badania, Wynik, Numer_VIN) VALUES
('CERT-2026-001', '2026-01-05', 'Pozytywny', '1FA6P8CF0H1234567'),
('CERT-2026-002', '2026-03-10', 'Pozytywny', 'WA1ABCDE2H7654321'),
('CERT-2026-003', '2026-02-22', 'Pozytywny', 'WBA3A51000K111111');
GO

PRINT '=== 6. WPROWADZANIE POLIS OC ===';
INSERT INTO Polisa_OC (Numer_polisy, Ubezpieczyciel, Data_waznosci, Numer_VIN) VALUES
('POL-PZU-99887', 'PZU SA', '2027-01-09', '1FA6P8CF0H1234567'), 
('POL-WARTA-5544', 'Warta', '2027-03-14', 'WA1ABCDE2H7654321'), 
('POL-ERGO-1122', 'Ergo Hestia', '2025-12-31', 'WBA3A51000K111111'); 
GO

PRINT '============================================================================';
PRINT '=== DANE WPROWADZONE POMYŚLNIE! MOŻESZ TERAZ URUCHOMIĆ PROGRAM. ===';
PRINT '============================================================================';
GO
