using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Models;
using Belt_calculation_Tg_bot.Models.State;
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

            using var cmd = new NpgsqlCommand(
                "INSERT INTO users (telegram_id, username, user_role) VALUES (@telegram_id, @username, @user_role)", conn);

            cmd.Parameters.AddWithValue("telegram_id", telegramId);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("user_role", "User");

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Exists, UserRole? Role)> GetUserByUsernameAsync(string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT user_role FROM users WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var roleString = reader.GetString(0);
                if (Enum.TryParse<UserRole>(roleString, out var role))
                    return (true, role);
            }

            return (false, null);
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

        public async Task AddBeltAsync(Belt belt)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                "INSERT INTO belts (name, weight, k1, k2, k3, k4, l0) VALUES (@name, @weight, @k1, @k2, @k3, @k4, @l0)",
                connection);

            command.Parameters.AddWithValue("name", belt.Name);
            command.Parameters.AddWithValue("weight", belt.Weight);
            command.Parameters.AddWithValue("k1", belt.K1);
            command.Parameters.AddWithValue("k2", belt.K2);
            command.Parameters.AddWithValue("k3", belt.K3);
            command.Parameters.AddWithValue("k4", belt.K4);
            command.Parameters.AddWithValue("l0", belt.L0);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteBeltAsync(int beltId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand("DELETE FROM belts WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("id", beltId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
