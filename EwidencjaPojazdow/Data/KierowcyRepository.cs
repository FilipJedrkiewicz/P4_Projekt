using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Data;

public class KierowcyRepository
{
    public List<Kierowca> GetAll()
    {
        var lista = new List<Kierowca>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT PESEL, Imie, Nazwisko, Miejscowosc, Data_urodzenia " +
            "FROM Kierowca_Wlasciciel ORDER BY Nazwisko, Imie", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new Kierowca
            {
                PESEL          = reader.GetString(0),
                Imie           = reader.GetString(1),
                Nazwisko       = reader.GetString(2),
                Miejscowosc    = reader.GetString(3),
                DataUrodzenia  = reader.GetDateTime(4)
            });
        return lista;
    }

    public void Dodaj(Kierowca k)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO Kierowca_Wlasciciel (PESEL, Imie, Nazwisko, Miejscowosc, Data_urodzenia) " +
            "VALUES (@PESEL, @Imie, @Nazwisko, @Miej, @Data)", conn);
        cmd.Parameters.AddWithValue("@PESEL", k.PESEL);
        cmd.Parameters.AddWithValue("@Imie",  k.Imie);
        cmd.Parameters.AddWithValue("@Nazwisko", k.Nazwisko);
        cmd.Parameters.AddWithValue("@Miej",  k.Miejscowosc);
        cmd.Parameters.AddWithValue("@Data",  k.DataUrodzenia);
        cmd.ExecuteNonQuery();
    }

    public void Aktualizuj(Kierowca k)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE Kierowca_Wlasciciel " +
            "SET Imie=@Imie, Nazwisko=@Nazwisko, Miejscowosc=@Miej, Data_urodzenia=@Data " +
            "WHERE PESEL=@PESEL", conn);
        cmd.Parameters.AddWithValue("@Imie",     k.Imie);
        cmd.Parameters.AddWithValue("@Nazwisko", k.Nazwisko);
        cmd.Parameters.AddWithValue("@Miej",     k.Miejscowosc);
        cmd.Parameters.AddWithValue("@Data",     k.DataUrodzenia);
        cmd.Parameters.AddWithValue("@PESEL",    k.PESEL);
        cmd.ExecuteNonQuery();
    }

    public void Usun(string pesel)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM Kierowca_Wlasciciel WHERE PESEL = @PESEL", conn);
        cmd.Parameters.AddWithValue("@PESEL", pesel);
        cmd.ExecuteNonQuery();
    }
}
