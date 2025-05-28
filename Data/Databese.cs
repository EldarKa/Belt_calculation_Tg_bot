using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Models;
using Npgsql;

namespace Belt_calculation_Tg_bot.Data
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("username", username);
            var count = (long)await cmd.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task<bool> AddUserAsync(long telegramId, string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("INSERT INTO users (telegram_id, username) VALUES (@id, @username)", conn);
            cmd.Parameters.AddWithValue("id", telegramId);
            cmd.Parameters.AddWithValue("username", username);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateUserAsync(string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("username", username);
            var count = (long)await cmd.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task<List<Belt>> GetAllBeltsAsync()
        {
            var belts = new List<Belt>();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT id, name, weight, k1, k2, k3, k4, l0 FROM belts";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                belts.Add(new Belt
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Weight = reader.GetDouble(2),
                    K1 = reader.GetDouble(3),
                    K2 = reader.GetDouble(4),
                    K3 = reader.GetDouble(5),
                    K4 = reader.GetDouble(6),
                    L0 = reader.GetDouble(7)
                });
            }

            return belts;
        }

        public async Task<Belt?> GetBeltByNameAsync(string name)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT id, name, weight, k1, k2, k3, k4, l0 FROM belts WHERE name = @name LIMIT 1";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("name", name);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Belt
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Weight = reader.GetDouble(2),
                    K1 = reader.GetDouble(3),
                    K2 = reader.GetDouble(4),
                    K3 = reader.GetDouble(5),
                    K4 = reader.GetDouble(6),
                    L0 = reader.GetDouble(7)
                };
            }

            return null;
        }
    }
}
