using KutuphaneOtomasyonu.Domain.Entities;

namespace KutuphaneOtomasyonu.Application.Interfaces;

public interface IAuthService
{
    Task<Member?> ValidateUserAsync(string email, string password);
    Task<Member?> GetUserByEmailAsync(string email);
    Task<Member?> GetUserByIdAsync(int id);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

