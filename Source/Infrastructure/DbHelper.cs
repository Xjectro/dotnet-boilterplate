using Npgsql;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Source.Infrastructure
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<NpgsqlConnection> CreateOpenConnectionAsync()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }

        public async Task<T?> GetSingleAsync<T>(string query, Func<NpgsqlDataReader, T> map, params NpgsqlParameter[] parameters)
        {
            await using var conn = await CreateOpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return map(reader);

            return default;
        }

        public async Task<List<T>> GetListAsync<T>(string query, Func<NpgsqlDataReader, T> map, params NpgsqlParameter[] parameters)
        {
            var result = new List<T>();
            await using var conn = await CreateOpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(map(reader));
            }
            return result;
        }

        public async Task<bool> ExistsAsync(string query, params NpgsqlParameter[] parameters)
        {
            await using var conn = await CreateOpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result ?? 0) > 0;
        }
        // Example mapping function usage:
        // var member = await dbHelper.GetSingleAsync("SELECT ...", r => new Member(...), ...);
    }
}