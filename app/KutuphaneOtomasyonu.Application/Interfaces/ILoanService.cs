using KutuphaneOtomasyonu.Application.DTOs;

namespace KutuphaneOtomasyonu.Application.Interfaces;

public interface ILoanService
{
    Task<int> LoanBookAsync(int copyId, int memberId, int createdBy, int loanDays);
    Task ReturnBookAsync(int loanId, int returnedBy);
    Task<List<ActiveLoanViewModel>> GetActiveLoansAsync();
    Task<List<AvailableCopyViewModel>> GetAvailableCopiesAsync();
    Task<int> GetActiveLoanCountAsync();
    Task<int> GetOverdueLoanCountAsync();
    Task<List<RecentLoanViewModel>> GetRecentLoansAsync(int count);
    
    // Member-specific methods
    Task<List<ActiveLoanViewModel>> GetMemberActiveLoansAsync(int memberId);
    Task<List<MemberLoanHistoryItem>> GetMemberLoanHistoryAsync(int memberId);
}

