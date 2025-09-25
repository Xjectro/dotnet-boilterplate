using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly AppDbContext _context;

    public MemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Members.AnyAsync(m => m.Username == username);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Members.AnyAsync(m => m.Email == email);
    }

    public async Task<Member?> GetByUsernameAsync(string username)
    {
        return await _context.Members
                             .FirstOrDefaultAsync(m => m.Username == username);
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _context.Members
                             .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _context.Members
                             .FirstOrDefaultAsync(m => m.Email == email);
    }

    public async Task AddAsync(Member member)
    {
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
    }
}
