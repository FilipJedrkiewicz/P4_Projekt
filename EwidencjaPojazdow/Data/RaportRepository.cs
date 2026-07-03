using System.Data;
using EwidencjaPojazdow.Database;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Data;

public class RaportRepository
{
    public DataTable GetRaport()
    {
        var tabela = new DataTable();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var adapter = new SqlDataAdapter(
            "SELECT * FROM VW_ZintegrowanyRaportPojazdu", conn);
        adapter.Fill(tabela);
        return tabela;
    }
}
