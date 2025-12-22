using KutuphaneOtomasyon.Models;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public interface ILoanService
    {
        Task<List<Loan>> GetAllLoansAsync();
        Task<List<Loan>> GetActiveLoansAsync();
        Task<List<Loan>> GetOverdueLoansAsync();
        Task<List<Loan>> GetLoansByMemberIdAsync(int memberId);
        Task<Loan?> GetLoanByIdAsync(int loanId);
        Task<int> LoanBookAsync(int copyId, int memberId, int createdBy, int loanDays = 14);
        Task ReturnBookAsync(int loanId, int returnedBy, decimal fineAmount = 0);
        Task<int> GetActiveLoanCountByMemberAsync(int memberId);
        Task<bool> CanMemberBorrowAsync(int memberId);
    }
}
