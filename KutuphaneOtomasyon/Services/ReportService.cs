using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyon.Data;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public class ReportService : IReportService
    {
        private readonly KutuphaneDbContext _context;

        public ReportService(KutuphaneDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var now = DateTime.Now;

            var dashboard = new DashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalMembers = await _context.Members.CountAsync(m => m.Status == "active"),
                ActiveLoans = await _context.Loans.CountAsync(l => l.ReturnedAt == null),
                OverdueLoans = await _context.Loans.CountAsync(l => l.ReturnedAt == null && l.DueAt < now)
            };

            // Son 5 ödünç
            dashboard.RecentLoans = await _context.Loans
                .Include(l => l.Copy)
                    .ThenInclude(c => c!.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.LoanedAt)
                .Take(5)
                .Select(l => new RecentLoanViewModel
                {
                    LoanId = l.LoanId,
                    BookTitle = l.Copy!.Book!.Title,
                    MemberName = l.Member!.FullName,
                    LoanedAt = l.LoanedAt,
                    DueAt = l.DueAt,
                    IsOverdue = l.ReturnedAt == null && l.DueAt < now
                })
                .ToListAsync();

            // Popüler kitaplar (son 30 gün)
            dashboard.PopularBooks = await GetTopBooksLast30DaysAsync(5);

            return dashboard;
        }

        public async Task<List<PopularBookViewModel>> GetTopBooksLast30DaysAsync(int count = 10)
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            return await _context.Loans
                .Where(l => l.LoanedAt >= thirtyDaysAgo)
                .Include(l => l.Copy)
                    .ThenInclude(c => c!.Book)
                .GroupBy(l => new { l.Copy!.Book!.BookId, l.Copy.Book.Title, l.Copy.Book.Author })
                .Select(g => new PopularBookViewModel
                {
                    BookId = g.Key.BookId,
                    Title = g.Key.Title,
                    Author = g.Key.Author,
                    LoanCount = g.Count()
                })
                .OrderByDescending(p => p.LoanCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<MemberLoanStatsViewModel>> GetMemberLoanStatsAsync()
        {
            var now = DateTime.Now;

            return await _context.Members
                .Select(m => new MemberLoanStatsViewModel
                {
                    MemberId = m.MemberId,
                    MemberName = m.FullName,
                    Email = m.Email,
                    TotalLoans = m.Loans.Count,
                    ActiveLoans = m.Loans.Count(l => l.ReturnedAt == null),
                    ReturnedLoans = m.Loans.Count(l => l.ReturnedAt != null),
                    OverdueLoans = m.Loans.Count(l => l.ReturnedAt == null && l.DueAt < now)
                })
                .Where(s => s.TotalLoans > 0)
                .OrderByDescending(s => s.TotalLoans)
                .ToListAsync();
        }
    }
}
