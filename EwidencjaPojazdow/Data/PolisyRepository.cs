using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Data;

public class PolisyRepository
{
    public List<PolisaOC> GetAll()
    {
        var lista = new List<PolisaOC>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT Numer_polisy, Ubezpieczyciel, Data_waznosci, Numer_VIN " +
            "FROM Polisa_OC ORDER BY Data_waznosci DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new PolisaOC
            {
                NumerPolisy    = reader.GetString(0),
                Ubezpieczyciel = reader.GetString(1),
                DataWaznosci   = reader.GetDateTime(2),
                NumerVIN       = reader.GetString(3)
            });
        return lista;
    }

    public void Dodaj(PolisaOC p)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO Polisa_OC (Numer_polisy, Ubezpieczyciel, Data_waznosci, Numer_VIN) " +
            "VALUES (@N, @U, @D, @V)", conn);
        cmd.Parameters.AddWithValue("@N", p.NumerPolisy);
        cmd.Parameters.AddWithValue("@U", p.Ubezpieczyciel);
        cmd.Parameters.AddWithValue("@D", p.DataWaznosci);
        cmd.Parameters.AddWithValue("@V", p.NumerVIN.ToUpper());
        cmd.ExecuteNonQuery();
    }

    public void Aktualizuj(PolisaOC p)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE Polisa_OC SET Ubezpieczyciel=@U, Data_waznosci=@D, Numer_VIN=@V " +
            "WHERE Numer_polisy=@N", conn);
        cmd.Parameters.AddWithValue("@U", p.Ubezpieczyciel);
        cmd.Parameters.AddWithValue("@D", p.DataWaznosci);
        cmd.Parameters.AddWithValue("@V", p.NumerVIN.ToUpper());
        cmd.Parameters.AddWithValue("@N", p.NumerPolisy);
        cmd.ExecuteNonQuery();
    }

    public void Usun(string numerPolisy)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM Polisa_OC WHERE Numer_polisy = @N", conn);
        cmd.Parameters.AddWithValue("@N", numerPolisy);
        cmd.ExecuteNonQuery();
    }
}
