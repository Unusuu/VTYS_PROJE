using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyon.Data;
using KutuphaneOtomasyon.Models;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public class LoanService : ILoanService
    {
        private readonly KutuphaneDbContext _context;

        public LoanService(KutuphaneDbContext context)
        {
            _context = context;
        }

        public async Task<List<Loan>> GetAllLoansAsync()
        {
            return await _context.Loans
                .Include(l => l.Copy)
                    .ThenInclude(c => c!.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.LoanedAt)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetActiveLoansAsync()
        {
            return await _context.Loans
                .Where(l => l.ReturnedAt == null)
                .Include(l => l.Copy)
                    .ThenInclude(c => c!.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.LoanedAt)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetOverdueLoansAsync()
        {
            var now = DateTime.Now;
            return await _context.Loans
                .Where(l => l.ReturnedAt == null && l.DueAt < now)
                .Include(l => l.Copy)
                    .ThenInclude(c => c!.Book)
                .Include(l => l.Member)
                .OrderBy(l => l.DueAt)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetLoansByMemberIdAsync(int memberId)
        {
            return await _context.Loans
                .Where(l => l.MemberId == memberId)
                .Include(l => l.Copy)
                    .ThenInclude(c => c!.Book)
                .OrderByDescending(l => l.LoanedAt)
                .ToListAsync();
        }

        public async Task<Loan?> GetLoanByIdAsync(int loanId)
        {
            return await _context.Loans
                .Include(l => l.Copy)
                    .ThenInclude(c => c!.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);
        }

        public async Task<int> LoanBookAsync(int copyId, int memberId, int createdBy, int loanDays = 14)
        {
            // İş kuralı kontrolü (R05): Limit kontrolü
            if (!await CanMemberBorrowAsync(memberId))
            {
                throw new InvalidOperationException("Üye ödünç alma limitine ulaşmış!");
            }

            var loanId = await _context.LoanBookAsync(copyId, memberId, createdBy, loanDays);
            return loanId;
        }

        public async Task ReturnBookAsync(int loanId, int returnedBy, decimal fineAmount = 0)
        {
            await _context.ReturnBookAsync(loanId, returnedBy, fineAmount);
        }

        public async Task<int> GetActiveLoanCountByMemberAsync(int memberId)
        {
            return await _context.Loans
                .CountAsync(l => l.MemberId == memberId && l.ReturnedAt == null);
        }

        public async Task<bool> CanMemberBorrowAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member == null || member.Status != "active")
                return false;

            var activeLoans = await GetActiveLoanCountByMemberAsync(memberId);
            return activeLoans < member.MaxLoanLimit;
        }
    }
}
