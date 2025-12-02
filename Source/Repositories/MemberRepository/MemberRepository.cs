using System.Data;
using System.Data.Common;
using Npgsql;

using Source.Models;
using Source.Infrastructure;

namespace Source.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly DbHelper _dbHelper;

    public MemberRepository(string connectionString)
    {
        _dbHelper = new DbHelper(connectionString);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _dbHelper.ExistsAsync(
            "SELECT COUNT(*) FROM members WHERE username = @username",
            new NpgsqlParameter("@username", username)
        );
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbHelper.ExistsAsync(
            "SELECT COUNT(*) FROM members WHERE email = @email",
            new NpgsqlParameter("@email", email)
        );
    }

    public async Task<Member?> GetByUsernameAsync(string username)
    {
        return await _dbHelper.GetSingleAsync(
            "SELECT id, username, email, password FROM members WHERE username = @username LIMIT 1",
            reader => new Member(
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("password"))
            )
            { Id = reader.GetGuid(reader.GetOrdinal("id")) },
            new NpgsqlParameter("@username", username)
        );
    }

    public async Task<Member?> GetByIdAsync(Guid id)
    {
        return await _dbHelper.GetSingleAsync(
            "SELECT id, username, email, password FROM members WHERE id = @id LIMIT 1",
            reader => new Member(
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("password"))
            )
            { Id = reader.GetGuid(reader.GetOrdinal("id")) },
            new NpgsqlParameter("@id", id)
        );
    }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _dbHelper.GetSingleAsync(
            "SELECT id, username, email, password FROM members WHERE email = @email LIMIT 1",
            reader => new Member(
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("password"))
            )
            { Id = reader.GetGuid(reader.GetOrdinal("id")) },
            new NpgsqlParameter("@email", email)
        );
    }

    public async Task AddAsync(Member member)
    {
        await using var conn = await _dbHelper.CreateOpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("INSERT INTO members (username, email, password) VALUES (@username, @email, @password)", conn);
        cmd.Parameters.AddWithValue("@username", member.Username);
        cmd.Parameters.AddWithValue("@email", member.Email);
        cmd.Parameters.AddWithValue("@password", member.Password);
        await cmd.ExecuteNonQueryAsync();
    }
}
