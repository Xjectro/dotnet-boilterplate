using Domain.Entities;

namespace Application.Interfaces;

public interface IMemberRepository
{
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Member?> GetByUsernameAsync(string username);
    Task<Member?> GetByEmailAsync(string email);
    Task<Member?> GetByIdAsync(int id);
    Task AddAsync(Member member);
}
