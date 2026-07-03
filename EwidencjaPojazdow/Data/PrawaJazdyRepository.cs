using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Data;

public class PrawaJazdyRepository
{
    public List<PrawoJazdy> GetAll()
    {
        var lista = new List<PrawoJazdy>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT Numer_druku, Kategorie, Data_wydania, PESEL " +
            "FROM Prawo_Jazdy ORDER BY Numer_druku", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new PrawoJazdy
            {
                NumerDruku  = reader.GetString(0),
                Kategorie   = reader.GetString(1),
                DataWydania = reader.GetDateTime(2),
                PESEL       = reader.GetString(3)
            });
        return lista;
    }

    public void Dodaj(PrawoJazdy p)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO Prawo_Jazdy (Numer_druku, Kategorie, Data_wydania, PESEL) " +
            "VALUES (@N, @K, @D, @P)", conn);
        cmd.Parameters.AddWithValue("@N", p.NumerDruku);
        cmd.Parameters.AddWithValue("@K", p.Kategorie);
        cmd.Parameters.AddWithValue("@D", p.DataWydania);
        cmd.Parameters.AddWithValue("@P", p.PESEL);
        cmd.ExecuteNonQuery();
    }

    public void Aktualizuj(PrawoJazdy p)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE Prawo_Jazdy SET Kategorie=@K, Data_wydania=@D, PESEL=@P " +
            "WHERE Numer_druku=@N", conn);
        cmd.Parameters.AddWithValue("@K", p.Kategorie);
        cmd.Parameters.AddWithValue("@D", p.DataWydania);
        cmd.Parameters.AddWithValue("@P", p.PESEL);
        cmd.Parameters.AddWithValue("@N", p.NumerDruku);
        cmd.ExecuteNonQuery();
    }

    public void Usun(string numerDruku)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM Prawo_Jazdy WHERE Numer_druku = @N", conn);
        cmd.Parameters.AddWithValue("@N", numerDruku);
        cmd.ExecuteNonQuery();
    }
}
