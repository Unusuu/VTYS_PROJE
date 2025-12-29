using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Infrastructure.Data;
using KutuphaneOtomasyonu.Application.DTOs;

namespace KutuphaneOtomasyonu.Application.Services;

public class ReportService : Interfaces.IReportService
{
    private readonly KutuphaneDbContext _context;

    public ReportService(KutuphaneDbContext context)
    {
        _context = context;
    }

    public async Task<List<TopBookViewModel>> GetTopBooksLast30DaysAsync()
    {
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);

        return await _context.Loans
            .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
            .Where(l => l.LoanedAt >= thirtyDaysAgo)
            .GroupBy(l => new { l.Copy.Book.BookId, l.Copy.Book.Title, l.Copy.Book.Author, l.Copy.Book.Category })
            .Select(g => new TopBookViewModel
            {
                BookId = g.Key.BookId,
                Title = g.Key.Title,
                Author = g.Key.Author,
                Category = g.Key.Category,
                LoanCount = g.Count()
            })
            .OrderByDescending(x => x.LoanCount)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<MemberLoanReportViewModel>> GetMemberLoanReportsAsync()
    {
        return await _context.Members
            .Select(m => new MemberLoanReportViewModel
            {
                MemberId = m.MemberId,
                FullName = m.FullName,
                Email = m.Email,
                TotalLoans = m.Loans.Count,
                ActiveLoans = m.Loans.Count(l => l.ReturnedAt == null),
                ReturnedLoans = m.Loans.Count(l => l.ReturnedAt != null),
                OverdueLoans = m.Loans.Count(l => l.ReturnedAt == null && l.DueAt < DateTime.Now)
            })
            .Where(m => m.TotalLoans > 0)
            .OrderByDescending(m => m.TotalLoans)
            .ToListAsync();
    }
}

