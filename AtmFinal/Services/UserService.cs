using AtmFinal.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace AtmFinal.Services
{
    public class UserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public User Login(string cardNumber, string pin)
        {
            string connStr = _configuration.GetConnectionString("MyDbConnection");

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "SELECT * FROM users WHERE card_number = @card AND pin = @pin";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@card", cardNumber);
            cmd.Parameters.AddWithValue("@pin", pin);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader["id"]),
                    CardNumber = reader["card_number"].ToString(),
                    Pin = reader["pin"].ToString(),
                    Balance = Convert.ToDecimal(reader["balance"]),
                    AccountName = reader["account_name"].ToString()
                };
            }

            return null;
        }

        public void UpdateBalance(int id, decimal newBalance)
        {
            string connStr = _configuration.GetConnectionString("MyDbConnection");

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "UPDATE users SET balance = @balance WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@balance", newBalance);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
