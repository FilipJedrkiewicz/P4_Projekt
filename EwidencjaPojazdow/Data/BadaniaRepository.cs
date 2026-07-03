using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Data;

public class BadaniaRepository
{
    public List<BadanieTechniczne> GetAll()
    {
        var lista = new List<BadanieTechniczne>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT Nr_zaswiadczenia, Data_badania, Wynik, Numer_VIN " +
            "FROM Badanie_Techniczne ORDER BY Data_badania DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new BadanieTechniczne
            {
                NrZaswiadczenia = reader.GetString(0),
                DataBadania     = reader.GetDateTime(1),
                Wynik           = reader.GetString(2),
                NumerVIN        = reader.GetString(3)
            });
        return lista;
    }

    public void Dodaj(BadanieTechniczne b)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO Badanie_Techniczne (Nr_zaswiadczenia, Data_badania, Wynik, Numer_VIN) " +
            "VALUES (@N, @D, @W, @V)", conn);
        cmd.Parameters.AddWithValue("@N", b.NrZaswiadczenia);
        cmd.Parameters.AddWithValue("@D", b.DataBadania);
        cmd.Parameters.AddWithValue("@W", b.Wynik);
        cmd.Parameters.AddWithValue("@V", b.NumerVIN.ToUpper());
        cmd.ExecuteNonQuery();
    }

    public void Aktualizuj(BadanieTechniczne b)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE Badanie_Techniczne SET Data_badania=@D, Wynik=@W, Numer_VIN=@V " +
            "WHERE Nr_zaswiadczenia=@N", conn);
        cmd.Parameters.AddWithValue("@D", b.DataBadania);
        cmd.Parameters.AddWithValue("@W", b.Wynik);
        cmd.Parameters.AddWithValue("@V", b.NumerVIN.ToUpper());
        cmd.Parameters.AddWithValue("@N", b.NrZaswiadczenia);
        cmd.ExecuteNonQuery();
    }

    public void Usun(string nrZaswiadczenia)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM Badanie_Techniczne WHERE Nr_zaswiadczenia = @N", conn);
        cmd.Parameters.AddWithValue("@N", nrZaswiadczenia);
        cmd.ExecuteNonQuery();
    }
}
