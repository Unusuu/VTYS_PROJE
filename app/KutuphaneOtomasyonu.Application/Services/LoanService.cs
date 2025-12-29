using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Infrastructure.Data;
using KutuphaneOtomasyonu.Application.DTOs;

namespace KutuphaneOtomasyonu.Application.Services;

public class LoanService : Interfaces.ILoanService
{
    private readonly KutuphaneDbContext _context;

    public LoanService(KutuphaneDbContext context)
    {
        _context = context;
    }

    public async Task<int> LoanBookAsync(int copyId, int memberId, int createdBy, int loanDays)
    {
        // Stored Procedure kullanarak ödünç ver
        return await _context.LoanBookAsync(copyId, memberId, createdBy, loanDays);
    }

    public async Task ReturnBookAsync(int loanId, int returnedBy)
    {
        // Stored Procedure kullanarak iade al
        await _context.ReturnBookAsync(loanId, returnedBy);
    }

    public async Task<List<ActiveLoanViewModel>> GetActiveLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
            .Include(l => l.Member)
            .Where(l => l.ReturnedAt == null)
            .OrderByDescending(l => l.LoanedAt)
            .Select(l => new ActiveLoanViewModel
            {
                LoanId = l.LoanId,
                CopyId = l.CopyId,
                BookTitle = l.Copy.Book.Title,
                Author = l.Copy.Book.Author,
                MemberName = l.Member.FullName,
                MemberEmail = l.Member.Email,
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt
            })
            .ToListAsync();
    }

    public async Task<List<AvailableCopyViewModel>> GetAvailableCopiesAsync()
    {
        return await _context.Copies
            .Include(c => c.Book)
            .Where(c => c.Status == "available")
            .OrderBy(c => c.Book.Title)
            .Select(c => new AvailableCopyViewModel
            {
                CopyId = c.CopyId,
                BookTitle = c.Book.Title,
                Author = c.Book.Author,
                ShelfLocation = c.ShelfLocation
            })
            .ToListAsync();
    }

    public async Task<int> GetActiveLoanCountAsync()
    {
        return await _context.Loans.CountAsync(l => l.ReturnedAt == null);
    }

    public async Task<int> GetOverdueLoanCountAsync()
    {
        return await _context.Loans.CountAsync(l => l.ReturnedAt == null && l.DueAt < DateTime.Now);
    }

    public async Task<List<RecentLoanViewModel>> GetRecentLoansAsync(int count)
    {
        return await _context.Loans
            .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
            .Include(l => l.Member)
            .OrderByDescending(l => l.LoanedAt)
            .Take(count)
            .Select(l => new RecentLoanViewModel
            {
                LoanId = l.LoanId,
                BookTitle = l.Copy.Book.Title,
                MemberName = l.Member.FullName,
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt,
                IsReturned = l.ReturnedAt != null
            })
            .ToListAsync();
    }

    public async Task<List<ActiveLoanViewModel>> GetMemberActiveLoansAsync(int memberId)
    {
        return await _context.Loans
            .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
            .Include(l => l.Member)
            .Where(l => l.MemberId == memberId && l.ReturnedAt == null)
            .OrderByDescending(l => l.LoanedAt)
            .Select(l => new ActiveLoanViewModel
            {
                LoanId = l.LoanId,
                CopyId = l.CopyId,
                BookTitle = l.Copy.Book.Title,
                Author = l.Copy.Book.Author,
                MemberName = l.Member.FullName,
                MemberEmail = l.Member.Email,
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt
            })
            .ToListAsync();
    }

    public async Task<List<MemberLoanHistoryItem>> GetMemberLoanHistoryAsync(int memberId)
    {
        return await _context.Loans
            .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.LoanedAt)
            .Select(l => new MemberLoanHistoryItem
            {
                LoanId = l.LoanId,
                BookTitle = l.Copy.Book.Title,
                Author = l.Copy.Book.Author,
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt,
                ReturnedAt = l.ReturnedAt
            })
            .ToListAsync();
    }
}


