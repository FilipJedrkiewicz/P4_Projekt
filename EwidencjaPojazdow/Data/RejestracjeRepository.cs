using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Data;

public class RejestracjeRepository
{
    public List<RejestracjaPojazdu> GetAll()
    {
        var lista = new List<RejestracjaPojazdu>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT Numer_rej, Data_rejestracji, Status, PESEL, Numer_VIN " +
            "FROM Rejestracja_Pojazdu ORDER BY Data_rejestracji DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new RejestracjaPojazdu
            {
                NumerRej         = reader.GetString(0),
                DataRejestracji  = reader.GetDateTime(1),
                Status           = reader.GetString(2),
                PESEL            = reader.GetString(3),
                NumerVIN         = reader.GetString(4)
            });
        return lista;
    }

    public void Dodaj(RejestracjaPojazdu r)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO Rejestracja_Pojazdu (Numer_rej, Data_rejestracji, Status, PESEL, Numer_VIN) " +
            "VALUES (@N, @D, @S, @P, @V)", conn);
        cmd.Parameters.AddWithValue("@N", r.NumerRej);
        cmd.Parameters.AddWithValue("@D", r.DataRejestracji);
        cmd.Parameters.AddWithValue("@S", r.Status);
        cmd.Parameters.AddWithValue("@P", r.PESEL);
        cmd.Parameters.AddWithValue("@V", r.NumerVIN.ToUpper());
        cmd.ExecuteNonQuery();
    }

    public void Aktualizuj(RejestracjaPojazdu r)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE Rejestracja_Pojazdu " +
            "SET Data_rejestracji=@D, Status=@S, PESEL=@P, Numer_VIN=@V " +
            "WHERE Numer_rej=@N", conn);
        cmd.Parameters.AddWithValue("@D", r.DataRejestracji);
        cmd.Parameters.AddWithValue("@S", r.Status);
        cmd.Parameters.AddWithValue("@P", r.PESEL);
        cmd.Parameters.AddWithValue("@V", r.NumerVIN.ToUpper());
        cmd.Parameters.AddWithValue("@N", r.NumerRej);
        cmd.ExecuteNonQuery();
    }

    public void Usun(string numerRej)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM Rejestracja_Pojazdu WHERE Numer_rej = @N", conn);
        cmd.Parameters.AddWithValue("@N", numerRej);
        cmd.ExecuteNonQuery();
    }
}
