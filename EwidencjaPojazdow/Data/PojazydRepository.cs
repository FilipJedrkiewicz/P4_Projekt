using EwidencjaPojazdow.Database;
using EwidencjaPojazdow.Models;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Data;

public class PojazydRepository
{
    public List<Pojazd> GetAll()
    {
        var lista = new List<Pojazd>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT Numer_VIN, Marka, Model, Rok_produkcji " +
            "FROM Pojazd ORDER BY Marka, Model", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new Pojazd
            {
                NumerVIN     = reader.GetString(0),
                Marka        = reader.GetString(1),
                Model        = reader.GetString(2),
                RokProdukcji = reader.GetInt32(3)
            });
        return lista;
    }

    public void Dodaj(Pojazd p)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO Pojazd (Numer_VIN, Marka, Model, Rok_produkcji) " +
            "VALUES (@V, @M, @Mo, @R)", conn);
        cmd.Parameters.AddWithValue("@V",  p.NumerVIN.ToUpper());
        cmd.Parameters.AddWithValue("@M",  p.Marka);
        cmd.Parameters.AddWithValue("@Mo", p.Model);
        cmd.Parameters.AddWithValue("@R",  p.RokProdukcji);
        cmd.ExecuteNonQuery();
    }

    public void Aktualizuj(Pojazd p)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE Pojazd SET Marka=@M, Model=@Mo, Rok_produkcji=@R " +
            "WHERE Numer_VIN=@V", conn);
        cmd.Parameters.AddWithValue("@M",  p.Marka);
        cmd.Parameters.AddWithValue("@Mo", p.Model);
        cmd.Parameters.AddWithValue("@R",  p.RokProdukcji);
        cmd.Parameters.AddWithValue("@V",  p.NumerVIN);
        cmd.ExecuteNonQuery();
    }

    public void Usun(string numerVIN)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM Pojazd WHERE Numer_VIN = @V", conn);
        cmd.Parameters.AddWithValue("@V", numerVIN);
        cmd.ExecuteNonQuery();
    }
}
