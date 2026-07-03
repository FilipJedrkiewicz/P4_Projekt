IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EwidencjaPojazdowDB')
BEGIN
    CREATE DATABASE EwidencjaPojazdowDB;
END;
GO

USE EwidencjaPojazdowDB;
GO

DROP TABLE IF EXISTS Polisa_OC;
DROP TABLE IF EXISTS Badanie_Techniczne;
DROP TABLE IF EXISTS Rejestracja_Pojazdu;
DROP TABLE IF EXISTS Pojazd;
DROP TABLE IF EXISTS Prawo_Jazdy;
DROP TABLE IF EXISTS Kierowca_Wlasciciel;
GO

CREATE TABLE Kierowca_Wlasciciel (
    PESEL CHAR(11) NOT NULL,
    Imie NVARCHAR(50) NOT NULL,
    Nazwisko NVARCHAR(50) NOT NULL,
    Miejscowosc NVARCHAR(100) NOT NULL,
    Data_urodzenia DATE NOT NULL,
    CONSTRAINT PK_Kierowca_Wlasciciel PRIMARY KEY (PESEL),
    CONSTRAINT CHK_Kierowca_PESEL CHECK (LEN(PESEL) = 11 AND PESEL NOT LIKE '%[^0-9]%'),
    CONSTRAINT CHK_Kierowca_Wiek CHECK (Data_urodzenia <= DATEADD(YEAR, -18, GETDATE()))
);

CREATE TABLE Prawo_Jazdy (
    Numer_druku VARCHAR(20) NOT NULL,
    Kategorie VARCHAR(50) NOT NULL,
    Data_wydania DATE NOT NULL,
    PESEL CHAR(11) NOT NULL,
    CONSTRAINT PK_Prawo_Jazdy PRIMARY KEY (Numer_druku),
    CONSTRAINT UQ_Prawo_Jazdy_Kierowca UNIQUE (PESEL),
    CONSTRAINT FK_Prawo_Jazdy_Kierowca FOREIGN KEY (PESEL) REFERENCES Kierowca_Wlasciciel(PESEL) ON DELETE CASCADE
);

CREATE TABLE Pojazd (
    Numer_VIN CHAR(17) NOT NULL,
    Marka NVARCHAR(50) NOT NULL,
    Model NVARCHAR(50) NOT NULL,
    Rok_produkcji INT NOT NULL,
    CONSTRAINT PK_Pojazd PRIMARY KEY (Numer_VIN),
    CONSTRAINT CHK_Pojazd_VIN CHECK (LEN(Numer_VIN) = 17),
    CONSTRAINT CHK_Pojazd_Rok CHECK (Rok_produkcji >= 1900 AND Rok_produkcji <= YEAR(GETDATE()))
);

CREATE TABLE Rejestracja_Pojazdu (
    Numer_rej VARCHAR(15) NOT NULL,
    Data_rejestracji DATE NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    PESEL CHAR(11) NOT NULL,
    Numer_VIN CHAR(17) NOT NULL,
    CONSTRAINT PK_Rejestracja_Pojazdu PRIMARY KEY (Numer_rej),
    CONSTRAINT FK_Rejestracja_Kierowca FOREIGN KEY (PESEL) REFERENCES Kierowca_Wlasciciel(PESEL),
    CONSTRAINT FK_Rejestracja_Pojazd FOREIGN KEY (Numer_VIN) REFERENCES Pojazd(Numer_VIN),
    CONSTRAINT CHK_Rejestracja_Status CHECK (Status IN ('Aktywny', 'Wyrejestrowany', 'Archiwalny'))
);

CREATE TABLE Badanie_Techniczne (
    Nr_zaswiadczenia VARCHAR(30) NOT NULL,
    Data_badania DATE NOT NULL,
    Wynik NVARCHAR(50) NOT NULL,
    Numer_VIN CHAR(17) NOT NULL,
    CONSTRAINT PK_Badanie_Techniczne PRIMARY KEY (Nr_zaswiadczenia),
    CONSTRAINT FK_Badanie_Pojazd FOREIGN KEY (Numer_VIN) REFERENCES Pojazd(Numer_VIN) ON DELETE CASCADE,
    CONSTRAINT CHK_Badanie_Wynik CHECK (Wynik IN ('Pozytywny', 'Negatywny'))
);

CREATE TABLE Polisa_OC (
    Numer_polisy VARCHAR(30) NOT NULL,
    Ubezpieczyciel NVARCHAR(100) NOT NULL,
    Data_waznosci DATE NOT NULL,
    Numer_VIN CHAR(17) NOT NULL,
    CONSTRAINT PK_Polisa_OC PRIMARY KEY (Numer_polisy),
    CONSTRAINT FK_Polisa_Pojazd FOREIGN KEY (Numer_VIN) REFERENCES Pojazd(Numer_VIN) ON DELETE CASCADE
);
GO

DROP VIEW IF EXISTS VW_ZintegrowanyRaportPojazdu;
GO

CREATE VIEW VW_ZintegrowanyRaportPojazdu AS
SELECT 
    P.Numer_VIN AS [Numer VIN],
    R.Numer_rej AS [Tablice Rejestracyjne],
    P.Marka AS [Marka],
    P.Model AS [Model],
    K.Imie + ' ' + K.Nazwisko AS [Aktualny Właściciel],
    ISNULL(CAST(MAX(B.Data_badania) AS VARCHAR(20)), 'Brak historii') AS [Ostatni Przegląd],
    ISNULL(CAST(MAX(O.Data_waznosci) AS VARCHAR(20)), 'Brak ubezpieczenia') AS [Koniec Polisy OC]
FROM Pojazd P
JOIN Rejestracja_Pojazdu R ON P.Numer_VIN = R.Numer_VIN
JOIN Kierowca_Wlasciciel K ON R.PESEL = K.PESEL
LEFT JOIN Badanie_Techniczne B ON P.Numer_VIN = B.Numer_VIN
LEFT JOIN Polisa_OC O ON P.Numer_VIN = O.Numer_VIN
WHERE R.Status = 'Aktywny'
GROUP BY P.Numer_VIN, R.Numer_rej, P.Marka, P.Model, K.Imie, K.Nazwisko;
GO

DROP FUNCTION IF EXISTS FN_WeryfikujWaznoscOC;
GO

CREATE FUNCTION FN_WeryfikujWaznoscOC (@VIN CHAR(17))
RETURNS NVARCHAR(30)
AS
BEGIN
    DECLARE @DataWaznosci DATE;
    DECLARE @Status NVARCHAR(30);

    SELECT TOP 1 @DataWaznosci = Data_waznosci 
    FROM Polisa_OC 
    WHERE Numer_VIN = @VIN 
    ORDER BY Data_waznosci DESC;

    IF @DataWaznosci IS NULL
        SET @Status = N'Brak danych o polisie';
    ELSE IF @DataWaznosci >= CAST(GETDATE() AS DATE)
        SET @Status = N'Polisa Aktualna';
    ELSE
        SET @Status = N'BRAK WAŻNEGO OC';

    RETURN @Status;
END;
GO

DROP TRIGGER IF EXISTS TR_Badanie_Chronologia;
GO

CREATE TRIGGER TR_Badanie_Chronologia
ON Badanie_Techniczne
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.Data_badania < (
            SELECT MIN(r.Data_rejestracji)
            FROM Rejestracja_Pojazdu r
            WHERE r.Numer_VIN = i.Numer_VIN
        )
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR ('Blad biznesowy: Data badania technicznego nie moze byc wczesniejsza niz data rejestracji pojazdu.', 16, 1);
        RETURN;
    END
END;
GO

DROP PROCEDURE IF EXISTS SP_ZarejestrujNowyPojazd;
GO

CREATE PROCEDURE SP_ZarejestrujNowyPojazd
    @Numer_rej VARCHAR(15),
    @Data_rejestracji DATE,
    @Status NVARCHAR(50),
    @PESEL CHAR(11),
    @Numer_VIN CHAR(17),
    @Marka NVARCHAR(50),
    @Model NVARCHAR(50),
    @Rok_produkcji INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Kierowca_Wlasciciel WHERE PESEL = @PESEL)
    BEGIN
        RAISERROR ('Blad walidacji: Podany kierowca/wlasciciel nie istnieje w bazie danych.', 16, 1);
        RETURN;
    END;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Pojazd WHERE Numer_VIN = @Numer_VIN)
        BEGIN
            INSERT INTO Pojazd (Numer_VIN, Marka, Model, Rok_produkcji)
            VALUES (@Numer_VIN, @Marka, @Model, @Rok_produkcji);
        END;

        INSERT INTO Rejestracja_Pojazdu (Numer_rej, Data_rejestracji, Status, PESEL, Numer_VIN)
        VALUES (@Numer_rej, @Data_rejestracji, @Status, @PESEL, @Numer_VIN);

        COMMIT TRANSACTION;
        PRINT 'Pojazd i rejestracja zostaly pomyslnie wprowadzone do systemu.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrMessage, 16, 1);
    END CATCH
END;
GO

PRINT '============================================================================';
