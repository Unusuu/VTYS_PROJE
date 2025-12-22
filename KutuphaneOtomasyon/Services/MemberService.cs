using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using KutuphaneOtomasyon.Data;
using KutuphaneOtomasyon.Models;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public class MemberService : IMemberService
    {
        private readonly KutuphaneDbContext _context;

        public MemberService(KutuphaneDbContext context)
        {
            _context = context;
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _context.Members
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Member>> GetActiveMembersAsync()
        {
            return await _context.Members
                .Where(m => m.Status == "active")
                .OrderBy(m => m.FullName)
                .ToListAsync();
        }

        public async Task<Member?> GetMemberByIdAsync(int memberId)
        {
            return await _context.Members
                .Include(m => m.Loans)
                .FirstOrDefaultAsync(m => m.MemberId == memberId);
        }

        public async Task<Member?> GetMemberByEmailAsync(string email)
        {
            return await _context.Members
                .FirstOrDefaultAsync(m => m.Email == email);
        }

        public async Task<int> AddMemberAsync(MemberViewModel model)
        {
            string? passwordHash = null;
            if (!string.IsNullOrEmpty(model.Password))
            {
                passwordHash = HashPassword(model.Password);
            }

            var memberId = await _context.AddMemberAsync(
                model.FullName,
                model.Email,
                model.Phone,
                model.Address,
                model.DateOfBirth,
                model.Role,
                passwordHash
            );
            return memberId;
        }

        public async Task UpdateMemberStatusAsync(int memberId, string status)
        {
            await _context.UpdateMemberStatusAsync(memberId, status);
        }

        public async Task<Member?> AuthenticateAsync(string email, string password)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email == email && m.Status == "active");

            if (member == null)
                return null;

            // Şifre kontrolü
            var hashedPassword = HashPassword(password);
            if (member.PasswordHash != hashedPassword)
                return null;

            return member;
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeMemberId = null)
        {
            var query = _context.Members.Where(m => m.Email == email);
            
            if (excludeMemberId.HasValue)
                query = query.Where(m => m.MemberId != excludeMemberId.Value);
            
            return await query.AnyAsync();
        }

        // Basit SHA256 hash (Production için BCrypt önerilir)
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
