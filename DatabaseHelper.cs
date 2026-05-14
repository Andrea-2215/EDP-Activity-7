using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ClothingStore
{
    /// <summary>
    /// PUBLIC database connection class — change the fields to match your MySQL setup.
    /// </summary>
    public class DatabaseHelper
    {
        // ── Change these to match your local MySQL / phpMyAdmin setup ──────────
        public static string Server = "127.0.0.1";
        public static string Database = "clothingstore";
        public static string User = "root";
        public static string Password = "";          // your phpMyAdmin/MariaDB password
        public static string Port = "3306";
        // ─────────────────────────────────────────────────────────────────────

        public static string ConnectionString =>
            $"server={Server};port={Port};database={Database};uid={User};pwd={Password};";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Simple connectivity test — returns true if the DB is reachable.</summary>
        public static bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    return conn.State == ConnectionState.Open;
                }
            }
            catch { return false; }
        }
    }
}