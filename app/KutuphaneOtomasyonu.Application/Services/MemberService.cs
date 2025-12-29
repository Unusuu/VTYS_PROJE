using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Infrastructure.Data;
using KutuphaneOtomasyonu.Domain.Entities;
using KutuphaneOtomasyonu.Application.DTOs;

namespace KutuphaneOtomasyonu.Application.Services;

public class MemberService : Interfaces.IMemberService
{
    private readonly KutuphaneDbContext _context;

    public MemberService(KutuphaneDbContext context)
    {
        _context = context;
    }

    public async Task<List<MemberListViewModel>> GetAllMembersAsync()
    {
        return await _context.Members
            .Select(m => new MemberListViewModel
            {
                MemberId = m.MemberId,
                FullName = m.FullName,
                Email = m.Email,
                Phone = m.Phone,
                Status = m.Status,
                Role = m.Role,
                JoinedAt = m.JoinedAt,
                ActiveLoanCount = m.Loans.Count(l => l.ReturnedAt == null)
            })
            .ToListAsync();
    }

    public async Task<List<Member>> GetActiveMembersAsync()
    {
        return await _context.Members
            .Where(m => m.Status == "active")
            .OrderBy(m => m.FullName)
            .ToListAsync();
    }

    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        return await _context.Members.FindAsync(id);
    }

    public async Task<int> AddMemberAsync(MemberCreateViewModel model)
    {
        // Stored Procedure kullanarak üye ekle
        return await _context.AddMemberAsync(
            model.FullName,
            model.Email,
            model.Phone,
            model.Address,
            model.DateOfBirth,
            model.Role,
            model.Password // Şifre
        );
    }

    public async Task UpdateMemberAsync(MemberEditViewModel model)
    {
        var member = await _context.Members.FindAsync(model.MemberId);
        if (member == null)
            throw new Exception("Üye bulunamadı!");

        // Admin rolündeki kullanıcılar düzenlenemez
        if (member.Role == "admin")
            throw new Exception("Admin rolündeki kullanıcılar düzenlenemez!");

        member.FullName = model.FullName;
        member.Email = model.Email;
        member.Phone = model.Phone;
        member.Address = model.Address;
        member.DateOfBirth = model.DateOfBirth;
        member.Role = model.Role;
        member.Status = model.Status;
        member.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task SetMemberStatusAsync(int memberId, string status)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
            throw new Exception("Üye bulunamadı!");

        // Admin rolündeki kullanıcılar pasif yapılamaz
        if (status == "inactive" && member.Role == "admin")
            throw new Exception("Admin rolündeki kullanıcılar pasif yapılamaz!");

        // Aktif ödünç kontrolü - pasif yapılacaksa
        if (status == "inactive")
        {
            var activeLoans = await _context.Loans
                .Where(l => l.MemberId == memberId && l.ReturnedAt == null)
                .CountAsync();

            if (activeLoans > 0)
                throw new Exception($"Bu üyenin {activeLoans} aktif ödüncü var. Önce kitaplar iade edilmelidir!");
        }

        member.Status = status;
        member.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int memberId, string currentPassword, string newPassword)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
            throw new Exception("Üye bulunamadı!");

        // Mevcut şifre kontrolü
        if (member.PasswordHash != currentPassword)
            throw new Exception("Mevcut şifre hatalı!");

        member.PasswordHash = newPassword;
        member.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task ResetPasswordByEmailAsync(string email, string newPassword)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
        if (member == null)
            throw new Exception("Bu e-posta adresiyle kayıtlı üye bulunamadı!");

        member.PasswordHash = newPassword;
        member.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetMemberCountAsync()
    {
        return await _context.Members.CountAsync();
    }
}

