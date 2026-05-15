using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ClothingStore
{
  
    public class DatabaseHelper
    {
       
        public static string Server = "127.0.0.1";
        public static string Database = "clothingstore";
        public static string User = "root";
        public static string Password = "";        
        public static string Port = "3306";
      

        public static string ConnectionString =>
            $"server={Server};port={Port};database={Database};uid={User};pwd={Password};";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

       
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
