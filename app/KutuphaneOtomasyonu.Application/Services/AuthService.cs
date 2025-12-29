using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Infrastructure.Data;
using KutuphaneOtomasyonu.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace KutuphaneOtomasyonu.Application.Services;

public class AuthService : Interfaces.IAuthService
{
    private readonly KutuphaneDbContext _context;

    public AuthService(KutuphaneDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> ValidateUserAsync(string email, string password)
    {
        var user = await _context.Members
            .FirstOrDefaultAsync(m => m.Email == email && m.Status == "active");

        if (user == null)
            return null;

        // Şifre kontrolü
        if (string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<Member?> GetUserByEmailAsync(string email)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.Email == email);
    }

    public async Task<Member?> GetUserByIdAsync(int id)
    {
        return await _context.Members.FindAsync(id);
    }

    public string HashPassword(string password)
    {
        // SHA256 ile basit hash (production'da BCrypt kullanılmalı)
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        // Önce plain text karşılaştırma (demo için)
        if (password == hash)
            return true;
            
        // Sonra hash karşılaştırma
        var passwordHash = HashPassword(password);
        return passwordHash == hash;
    }
}

