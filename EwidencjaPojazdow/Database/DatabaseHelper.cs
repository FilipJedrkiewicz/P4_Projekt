using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace EwidencjaPojazdow.Database;

public static class DatabaseHelper
{
    private static string _connectionString = LoadConnectionString();

    public static string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value;
    }

    private static string LoadConnectionString()
    {
        string cfgPath = Path.Combine(AppContext.BaseDirectory, "cepik.cfg");

        if (File.Exists(cfgPath))
        {
            var cfg = JsonSerializer.Deserialize<DbConfig>(File.ReadAllText(cfgPath))
                      ?? throw new InvalidOperationException("Nieprawidłowy plik cepik.cfg");

            var builder = new SqlConnectionStringBuilder
            {
                DataSource             = cfg.Server,
                InitialCatalog         = cfg.Database,
                TrustServerCertificate = true
            };

            if (cfg.IntegratedSecurity)
                builder.IntegratedSecurity = true;
            else
            {
                builder.UserID   = cfg.UserId;
                builder.Password = cfg.Password;
            }

            return builder.ConnectionString;
        }

        // Fallback — domyślne połączenie lokalne
        return "Server=localhost;Database=EwidencjaPojazdowDB;Integrated Security=True;TrustServerCertificate=True;";
    }

    public static SqlConnection GetConnection() => new(_connectionString);

    public static bool TestConnection()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class DbConfig
    {
        public string Server             { get; set; } = "localhost";
        public string Database           { get; set; } = "EwidencjaPojazdowDB";
        public bool   IntegratedSecurity { get; set; } = true;
        public string UserId             { get; set; } = "";
        public string Password           { get; set; } = "";
    }
}
